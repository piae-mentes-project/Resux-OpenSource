// Native Audio
// 5argon - Exceed7 Experiments
// Problems/suggestions : 5argon@exceed7.com

// Special thanks to Con for written this wonderful OpenAL tutorial : http://ohno789.blogspot.com/2013/08/playing-audio-samples-using-openal-on.html

#import "NativeAudio.h"

//#define LOG_NATIVE_AUDIO

@implementation NativeAudio

static ALCdevice *openALDevice;
static ALCcontext *openALContext;

//OpenAL sources starts at number 2400
//Hard limit specified by the library is that we can use up to 32 sources.
#define kMaxConcurrentSources 32

//We split stereo to separated sources, so we could do "balance panning" by adjusting each source's volume and without messing with OpenAL's positional audio to simulate panning.
//Unfortunately this reduces the usable sources from 32 to 16.

//For some reason, OpenAL prints this
//2019-03-16 00:02:09.703867+0700 ProductName[239:4538] AUBase.cpp:832:DispatchSetProperty:  ca_require: ValidFormat(inScope, inElement, newDesc) InvalidFormat
//Only while playing an audio rapidly over a certain frequency, once every 16th play, and cause only left ear to be audible at that play O_o
//I am not sure what is going on or is it OpenAL's bug, but reducing it to 15 works O_o
//Something must be defective at that 32th source, and that belongs to the right ear by our stereo splitting gymnastic explained earlier. I don't know the reason, but for now please bear with 1 less source.
//Wtf...
#define kHalfMaxConcurrentSources 15

//OpenAL buffer index starts at number 2432. (Now you know then source limit is implicitly 32)
//This number will be remembered in NativeAudioPointer at managed side.
//As far as I know there is no limit. I can allocate way over 500 sounds and it does not seems to cause any bad things.
//But of course every sound will cost memory and that should be your real limit.
//This is limit for just in case someday we discover a real hard limit, then Native Audio could warn us.
#define kMaxBuffers 1024

#define fixedMixingRate 24000

//Error when this goes to max
static int bufferAllocationCount = 0;
//Never reset
static int runningBufferAllocationNumber = 0;

static NativeAudioSourceIdPair* nasips;
static NativeAudioBufferIdPair* nabips;

+(void) naHandleInterruption: (NSNotification*) notification
{
    id audioInterruption = [[notification userInfo] valueForKey:AVAudioSessionInterruptionTypeKey];
    if(audioInterruption != NULL)
    {
        AVAudioSessionInterruptionType typeKey = (AVAudioSessionInterruptionType) [audioInterruption integerValue];
        if(typeKey == AVAudioSessionInterruptionTypeBegan)
        {
            //NSLog(@"--- INTERRUPTION ---");
            alcMakeContextCurrent(NULL);
        }
        else
        {
            //There is an iOS bug.. if you end the incoming call TOO FAST or unlucky (?) then end interruption won't be called lol
            //In that case we handle it with device becoming active notification instead.
            //NSLog(@"--- END INTERRUPTION ---");
            alcMakeContextCurrent(openALContext);
        }
    }
}

+(void) naDidBecomeActive: (NSNotification*) notification
{
    //NSLog(@"--- END INTERRUPTION (interruption end bugged) ---");
    //If the context was set before on interruption,
    //I think it is fine to set it again.
    alcMakeContextCurrent(openALContext);
}

+ (AudioFileID) openAudioFile:(NSString *)audioFilePathAsString
{
    NSURL *audioFileURL = [NSURL fileURLWithPath:audioFilePathAsString];
    
    AudioFileID afid;
    OSStatus openAudioFileResult = AudioFileOpenURL((__bridge CFURLRef)audioFileURL, kAudioFileReadPermission, 0, &afid);
    if (0 != openAudioFileResult)
    {
        NSLog(@"An error occurred when attempting to open the audio file %@: %d", audioFilePathAsString, (int)openAudioFileResult);
    }
    
    return afid;
}

+ (UInt32) getSizeOfAudioComponent:(AudioFileID)afid
{
    UInt64 audioDataSize = 0;
    UInt32 propertySize = sizeof(UInt64);
    
    OSStatus getSizeResult = AudioFileGetProperty(afid, kAudioFilePropertyAudioDataByteCount, &propertySize, &audioDataSize);
    
    if (0 != getSizeResult)
    {
        NSLog(@"An error occurred when attempting to determine the size of audio file.");
    }
    
    return (UInt32)audioDataSize;
}

+ (AudioStreamBasicDescription) getDescription:(AudioFileID)afid
{
    AudioStreamBasicDescription desc;
    UInt32 propertySize = sizeof(desc);
    
    OSStatus getSizeResult = AudioFileGetProperty(afid, kAudioFilePropertyDataFormat, &propertySize, &desc);
    
    if (0 != getSizeResult)
    {
        NSLog(@"An error occurred when attempting to determine the property of audio file.");
    }
    
    return desc;
}

+ (int) Initialize
{
    openALDevice = alcOpenDevice(NULL);
    openALContext = alcCreateContext(openALDevice, NULL);
    
    /*
     ALCint attributes[] =
     {
     ALC_FREQUENCY, fixedMixingRate
     };
     */
    //openALContext = alcCreateContext(openALDevice, attributes);
    
    alcMakeContextCurrent(openALContext);
    
    NSNotificationCenter* notiCenter = [NSNotificationCenter defaultCenter];
    //This is for handling phone calls, etc. Unity already handles AVAudioSession I think, we then handle OpenAL additionally.
    [notiCenter addObserver:self selector:@selector(naHandleInterruption:) name:AVAudioSessionInterruptionNotification object:NULL];
    
    //This is to handle iOS bug where if you end the phone call too fast or unlucky the interruption ended will not be called.
    //So DidBecomeActive will be an another safety net for us. Interruption began can't be missed I think, so it is safe
    //not to register WillResignActive.. (?)
    //[notiCenter addObserver:self selector:@selector(naWillResignActive:) name:UIApplicationWillResignActiveNotification object:NULL];
    [notiCenter addObserver:self selector:@selector(naDidBecomeActive:) name:UIApplicationDidBecomeActiveNotification object:NULL];
    
    nasips = (NativeAudioSourceIdPair*) malloc(sizeof(NativeAudioSourceIdPair) * kHalfMaxConcurrentSources);
    
    //"nabip" is for that just a single number can maps to 2 number (L and R buffer)
    //The upper limit of buffers is a whopping 1024, this will take 4096 bytes = 0.0041MB
    //I tried the realloc way, but it strangely realloc something related to text display Unity is using and crash the game (bug?)
    //Might be related to that the memory area is in the heap (static)
    nabips = (NativeAudioBufferIdPair*) malloc(sizeof(NativeAudioBufferIdPair*) * kMaxBuffers);
    
    ALuint sourceIDL;
    ALuint sourceIDR;
    for (int i = 0; i < kHalfMaxConcurrentSources; i++) {
        alGenSources(1, &sourceIDL);
        alSourcei(sourceIDL, AL_SOURCE_RELATIVE, AL_TRUE);
        alSourcef(sourceIDL, AL_REFERENCE_DISTANCE, 1.0f);
        alSourcef(sourceIDL, AL_MAX_DISTANCE, 2.0f);
        alGenSources(1, &sourceIDR);
        alSourcei(sourceIDR, AL_SOURCE_RELATIVE, AL_TRUE);
        alSourcef(sourceIDR, AL_REFERENCE_DISTANCE, 1.0f);
        alSourcef(sourceIDR, AL_MAX_DISTANCE, 2.0f);
        
        NativeAudioSourceIdPair nasip;
        nasip.left = sourceIDL;
        nasip.right = sourceIDR;
        nasips[i] = nasip;
        
        //roll off factor is default to 1.0
    }
    
    alDistanceModel(AL_LINEAR_DISTANCE_CLAMPED);
    
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Initialized OpenAL");
#endif
    
    return 0; //0 = success
}

+ (void) UnloadAudio: (int) index
{
    ALuint bufferIdL = (ALuint)nabips[index].left;
    ALuint bufferIdR = (ALuint)nabips[index].right;
    alDeleteBuffers(1, &bufferIdL);
    alDeleteBuffers(1, &bufferIdR);
    bufferAllocationCount -= 2;
}

+ (int) LoadAudio:(char*) soundUrl resamplingQuality:(int) resamplingQuality
{
    if (bufferAllocationCount > kMaxBuffers) {
        NSLog(@"Fail to load because OpenAL reaches the maximum sound buffers limit. Raise the limit or use unloading to free up the quota.");
        return -1;
    }
    
    if(openALDevice == nil)
    {
        [NativeAudio Initialize];
    }
    
    NSString *audioFilePath = [NSString stringWithFormat:@"%@/Data/Raw/%@", [[NSBundle mainBundle] resourcePath], [NSString stringWithUTF8String:soundUrl] ];
    
    AudioFileID afid = [NativeAudio openAudioFile:audioFilePath];
    AudioStreamBasicDescription loadingAudioDescription = [NativeAudio getDescription:afid];
    
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Input description : Flags %u Bits/channel %u FormatID %u SampleRate %f Bytes/Frame %u Bytes/Packet %u Channels/Frame %u Frames/Packet %u",
          (unsigned int)loadingAudioDescription.mFormatFlags,
          (unsigned int)loadingAudioDescription.mBitsPerChannel,
          (unsigned int)loadingAudioDescription.mFormatID,
          loadingAudioDescription.mSampleRate,
          (unsigned int)loadingAudioDescription.mBytesPerFrame,
          (unsigned int)loadingAudioDescription.mBytesPerPacket,
          (unsigned int)loadingAudioDescription.mChannelsPerFrame,
          (unsigned int)loadingAudioDescription.mFramesPerPacket
          );
#endif
    
    //UInt32 bytesPerFrame = loadingAudioDescription.mBytesPerFrame;
    UInt32 channel = loadingAudioDescription.mChannelsPerFrame;
    
    //This is originally float?
    //NSLog(@"LOADED RATE : %f", loadingAudioDescription.mSampleRate);
    //NSLog(@"CHANN : %u", (unsigned int)loadingAudioDescription.mChannelsPerFrame);
    //NSLog(@"BPF : %u", (unsigned int)loadingAudioDescription.mBytesPerFrame);
    UInt32 samplingRate = (UInt32) loadingAudioDescription.mSampleRate;
    
    //Next, load the original audio
    UInt32 audioSize = [NativeAudio getSizeOfAudioComponent:afid];
    char *audioData = (char*)malloc(audioSize);
    
    OSStatus readBytesResult = AudioFileReadBytes(afid, false, 0, &audioSize, audioData);
    
    if (0 != readBytesResult)
    {
        NSLog(@"ERROR : AudioFileReadBytes %@: %d", audioFilePath, (int)readBytesResult);
    }
    AudioFileClose(afid);
    
    int loadedIndex = [NativeAudio SendByteArray:audioData audioSize:audioSize channels:channel samplingRate:samplingRate resamplingQuality:resamplingQuality];
    
    if (audioData)
    {
        free(audioData);
        audioData = NULL;
    }
    
    return loadedIndex;
}

//Can call from Unity to give Unity-loaded AudioClip!!
+ (int) SendByteArray:(char*) audioData audioSize:(int)audioSize channels:(int)channel samplingRate:(int)samplingRate resamplingQuality:(int)resamplingQuality
{
    //I don't know if an "optimal rate" exist on Apple device or not.
    
    //Enable either one to make the 24000 that Unity choose matters, and resample our audio to match
    //This is always 24000 for all Unity games as far as I tried. (why?)
    //int rate = (int)[[AVAudioSession sharedInstance]sampleRate];
    //int rate = fixedMixingRate;
    
    //Enable this to not care, and just put our audio to OpenAL without resampling.
    int rate = samplingRate;
    
    if(samplingRate != rate)
    {
        float ratio = rate / ((float) samplingRate);
        
        //byte -> short
        size_t shortLength = audioSize / 2;
        
        size_t resampledArrayShortLength = (size_t)floor(shortLength * ratio);
        resampledArrayShortLength += resampledArrayShortLength % 2;
        
        NSLog(@"Resampling! Ratio %f / Length %zu -> %zu", ratio, shortLength, resampledArrayShortLength);
        
        float *floatArrayForSRCIn = (float*)calloc(shortLength, sizeof(float *));
        float *floatArrayForSRCOut = (float*)calloc(resampledArrayShortLength, sizeof(float *));
        
        //SRC takes float data.
        src_short_to_float_array((short*)audioData, floatArrayForSRCIn, (int)shortLength);
        
        SRC_DATA dataForSRC;
        dataForSRC.data_in = floatArrayForSRCIn;
        dataForSRC.data_out = floatArrayForSRCOut;
        
        dataForSRC.input_frames = shortLength / channel;
        dataForSRC.output_frames = resampledArrayShortLength / channel;
        dataForSRC.src_ratio = ratio; //This is in/out and it is less than 1.0 in the case of upsampling.
        
        //Use the SRC library. Thank you Eric!
        int error = src_simple(&dataForSRC, resamplingQuality, channel);
        if(error != 0)
        {
            [NSException raise:@"Native Audio Error" format:@"Resampling error with code %s", src_strerror(error)];
        }
        
        short* shortData = (short*)calloc(resampledArrayShortLength, sizeof(short *));
        src_float_to_short_array(floatArrayForSRCOut, shortData, (int)resampledArrayShortLength);
        shortLength = resampledArrayShortLength;
        
        //Replace the input argument with a new calloc.
        //We don't release the input argument, but in the case of resample we need to release it too.
        
        audioData = (char*)shortData;
        audioSize = (int)(resampledArrayShortLength * 2);
        
        free(floatArrayForSRCIn);
        free(floatArrayForSRCOut);
    }
    
    //I have a failed attempt to use AudioConverterFillComplexBuffer, a method where an entire internet does not have a single understandable working example.
    //If you want to do the "elegant" conversion, this is a very important read. (terminology, etc.)
    //https://developer.apple.com/documentation/coreaudio/audiostreambasicdescription
    //The deinterleaving conversion below is super noob and ugly... but it works.
    
    UInt32 bytesPerFrame = 2 * channel; // We fixed to 16-bit audio so that's that.
    UInt32 step = bytesPerFrame / channel;
    char *audioDataL = (char*)malloc(audioSize/channel);
    char *audioDataR = (char*)malloc(audioSize/channel);
    
    //NSLog(@"LR Length %d AudioSize %d Channel %d" , audioSize/channel, audioSize, channel );
    
    //This routine ensure no matter what the case `audioData` is completely migrated to the new L R separated buffer.
    if(channel == 2)
    {
        BOOL rightInterleave = false;
        // 0 1 2 3 4 5 6 7 8 9 101112131415
        // 0 1 0 1 2 3 2 3 4 5 4 5 6 7 6 7
        // L L R R L L R R L L R R L L R R
        for(int i = 0; i < audioSize; i += step)
        {
            int baseIndex = (i/bytesPerFrame) * step; //the divide will get rid of fractions first
            //NSLog(@"%d %d %u %d",i,baseIndex, (unsigned int)step, rightInterleave);
            for(int j = 0; j < step ; j++)
            {
                if(!rightInterleave)
                {
                    audioDataL[baseIndex + j] = audioData[i + j];
                }
                else
                {
                    audioDataR[baseIndex + j] = audioData[i + j];
                }
            }
            rightInterleave = !rightInterleave;
        }
    }
    else if(channel == 1)
    {
        for(int i = 0; i < audioSize; i++)
        {
            audioDataL[i] = audioData[i];
            audioDataR[i] = audioData[i];
        }
    }
    else
    {
        //throw?
        [NSException raise:@"Native Audio Error" format:@"Your audio is neither 1 nor 2 channels!"];
    }
    
    ALuint bufferIdL;
    alGenBuffers(1, &bufferIdL);
    bufferAllocationCount++;
    alBufferData(bufferIdL, AL_FORMAT_MONO16, audioDataL, audioSize/channel, rate);
    
    ALuint bufferIdR;
    alGenBuffers(1, &bufferIdR);
    bufferAllocationCount++;
    alBufferData(bufferIdR, AL_FORMAT_MONO16, audioDataR, audioSize/channel, rate);
    
    //alBufferData should be copying the audio to memory, so we can safely release them now.
    if (audioDataL)
    {
        free(audioDataL);
        audioDataL = NULL;
    }
    if (audioDataR)
    {
        free(audioDataR);
        audioDataR = NULL;
    }
    
    if(samplingRate != rate)
    {
        //This is now the new calloc-ed memory from the resampler. We can remove it.
        free(audioData);
        
        //Otherwise we cannot free the incoming data, since it is the same as it might be from C#.
        //We let C# GC handle it.
    }
    
    runningBufferAllocationNumber++;
    
    NativeAudioBufferIdPair nabip;
    nabip.left = bufferIdL;
    nabip.right = bufferIdR;
    
    //Calculate and cache other data
    nabip.channels = channel;
    nabip.bitDepth = 16;
    
    //This byte size is already stereo
    nabip.lengthSeconds = audioSize / (float)nabip.channels / (float)(nabip.bitDepth / 8) / (float)rate;
    
    nabips[runningBufferAllocationNumber - 1] = nabip;
    
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Loaded OpenAL sound: %@ bufferId: L %d R %d size: %u",[NSString stringWithUTF8String:soundUrl], bufferIdL, bufferIdR, (unsigned int)audioSize);
#endif
    
    return runningBufferAllocationNumber - 1;
}


static ALuint sourceCycleIndex = 0;

//Sources are selected sequentially.
//Searching for non-playing source might be a better idea to reduce sound cutoff chance
//(For example, by the time we reach 33rd sound some sound earlier must have finished playing, and we can select that one safely)
//But for performance concern I don't want to run a for...in loop everytime I play sounds.
//The reason of "half" of total available sources is this is only for the left channel. The right channel will be the left's index *2
+ (int) CycleThroughSources
{
    sourceCycleIndex = (sourceCycleIndex + 1) % kHalfMaxConcurrentSources;
    return sourceCycleIndex;
}

+ (float)LengthByAudioBuffer:(int)index
{
    return nabips[index].lengthSeconds;
}

+ (void)StopAudio:(int) nativeSourceIndex
{
    alSourceStop(nasips[nativeSourceIndex].left);
    alSourceStop(nasips[nativeSourceIndex].right);
}

// Confirm an incoming index. If invalid, fallback to round-robin.
// At C# side some methods sending in -1 to be intentionally invalid here so you get round-robin.
+ (int)GetNativeSource:(int) index
{
    //-1 or invalid source cycle will get a round robin play.
    if(index >= kHalfMaxConcurrentSources || index < 0)
    {
        index = [NativeAudio CycleThroughSources];
    }
    return index;
}

//Not only called from C# manual prepare, also internally from normal play as well.
+ (void)PrepareAudio:(int) audioBufferIndex IntoNativeSourceIndex:(int) nativeSourceIndex
{
    
    NativeAudioSourceIdPair nasip = nasips[nativeSourceIndex];
    NativeAudioBufferIdPair nabip = nabips[audioBufferIndex];
    
    //We cannot change audio source if it is playing, it will fail silently.
    alSourceStop(nasip.left);
    alSourceStop(nasip.right);
    
    alSourcei(nasip.left, AL_BUFFER, nabip.left);
    alSourcei(nasip.right, AL_BUFFER, nabip.right);
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Pairing OpenAL buffer: L %d R %d with source : L %d R %d", nabips[audioBufferIndex].left, nabips[audioBufferIndex].right, nasips[nativeSourceIndex].left, nasips[nativeSourceIndex].right);
#endif
}

//This is playing "blindly", believing that the audio assigned is correct. Must be used immediately after prepare for reliable result.
//It is separated to allow extreme micro optimization where you prepare in anticipation, then play later believing that
//it is still valid.
+ (void)PlayAudioWithNativeSourceIndex:(int) nativeSourceIndex Adjustment:(NativeAudioPlayAdjustment) playAdjustment
{
    NativeAudioSourceIdPair nasip = nasips[nativeSourceIndex];
    
    //If we call play before the adjust, you might hear the pre-adjusted audio.
    //It is THAT fast, even in between lines of code you can hear the audio already.
    [NativeAudio SetVolume:playAdjustment.volume OnNativeSourceIndex:nativeSourceIndex];
    [NativeAudio SetPan:playAdjustment.pan OnNativeSourceIndex:nativeSourceIndex];
    alSourcef(nasips[nativeSourceIndex].left, AL_SEC_OFFSET, playAdjustment.offsetSeconds);
    alSourcef(nasips[nativeSourceIndex].right, AL_SEC_OFFSET, playAdjustment.offsetSeconds);
    alSourcei(nasips[nativeSourceIndex].left, AL_LOOPING, playAdjustment.trackLoop ? AL_TRUE : AL_FALSE);
    alSourcei(nasips[nativeSourceIndex].right, AL_LOOPING, playAdjustment.trackLoop ? AL_TRUE : AL_FALSE);
    
    ALint state;
    //alSourcePlay on a paused source results in RESUME, we need to stop it to start over.
    alGetSourcei(nasips[nativeSourceIndex].left, AL_SOURCE_STATE, &state);
    if(state == AL_PAUSED)
    {
        alSourceStop(nasip.left);
        alSourceStop(nasip.right);
    }
    alSourcePlay(nasip.left);
    alSourcePlay(nasip.right);
    
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Played OpenAL at source index : L %d R %d", nasip.left, nasip.right);
#endif
}

+ (void)SetVolume:(float) volume OnNativeSourceIndex:(int) nativeSourceIndex
{
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Set Volume %f L %d R %d", volume, nasips[nativeSourceIndex].left, nasips[nativeSourceIndex].right);
#endif
    alSourcef(nasips[nativeSourceIndex].left, AL_GAIN, volume);
    alSourcef(nasips[nativeSourceIndex].right, AL_GAIN, volume);
}

//With OpenAL's 3D design, to achieve 2D panning we have deinterleaved the stereo file
//into 2 separated mono sources positioned left and right of the listener. This achieve the same stereo effect.
//Gain is already used in SetVolume, we will use a linear attenuation for panning.
+ (void)SetPan:(float) pan OnNativeSourceIndex:(int) nativeSourceIndex
{
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Set Pan %f L %d R %d", pan, nasips[nativeSourceIndex].left, nasips[nativeSourceIndex].right);
#endif
    //Left channel attenuate linearly on right pan
    alSource3f(nasips[nativeSourceIndex].left, AL_POSITION, -1 - (MAX(pan, 0)), 0, 0);
    //Right channel attenuate linearly on left pan
    alSource3f(nasips[nativeSourceIndex].right, AL_POSITION, 1 - (MIN(pan, 0)), 0, 0);
}

//Only one side is enough?
+ (float)GetPlaybackTimeOfNativeSourceIndex:(int) nativeSourceIndex
{
    ALfloat returnValue;
    alGetSourcef(nasips[nativeSourceIndex].left, AL_SEC_OFFSET, &returnValue);
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Get Playback Time %f", returnValue);
#endif
    return returnValue;
}

+(void)SetPlaybackTimeOfNativeSourceIndex:(int) nativeSourceIndex Offset:(float)offsetSeconds
{
    alSourcef(nasips[nativeSourceIndex].left, AL_SEC_OFFSET, offsetSeconds);
    alSourcef(nasips[nativeSourceIndex].right, AL_SEC_OFFSET, offsetSeconds);
    ALint state;
    alGetSourcei(nasips[nativeSourceIndex].left, AL_SOURCE_STATE, &state);
    if(state == AL_STOPPED)
    {
        alSourcePlay(nasips[nativeSourceIndex].left);
        alSourcePlay(nasips[nativeSourceIndex].right);
    }
#ifdef LOG_NATIVE_AUDIO
    NSLog(@"Set Playback Time %f", offsetSeconds);
#endif
}

+(void)Pause:(int)nativeSourceIndex
{
    ALint state;
    alGetSourcei(nasips[nativeSourceIndex].left, AL_SOURCE_STATE, &state);
    if(state == AL_PLAYING)
    {
        alSourcePause(nasips[nativeSourceIndex].left);
        alSourcePause(nasips[nativeSourceIndex].right);
    }
}

+(void) Resume:(int)nativeSourceIndex
{
    ALint state;
    alGetSourcei(nasips[nativeSourceIndex].left, AL_SOURCE_STATE, &state);
    if(state == AL_PAUSED)
    {
        alSourcePlay(nasips[nativeSourceIndex].left);
        alSourcePlay(nasips[nativeSourceIndex].right);
    }
}

//This is matched with what's defined at C#.
typedef enum IosAudioPortType
{
    //---Output---
    
    /// <summary>
    /// Line-level output to the dock connector.
    /// </summary>
    LineOut = 0,
    
    /// <summary>
    /// Output to a wired headset.
    /// </summary>
    Headphones = 1,
    
    /// <summary>
    /// Output to a speaker intended to be held near the ear.
    /// </summary>
    BuiltInReceiver = 2,
    
    /// <summary>
    /// Output to the device's built-in speaker.
    /// </summary>
    BuiltInSpeaker = 3,
    
    
    /// <summary>
    /// Output to a device via the High-Definition Multimedia Interface (HDMI) specification.
    /// </summary>
    HDMI = 4,
    
    /// <summary>
    /// Output to a remote device over AirPlay.
    /// </summary>
    AirPlay = 5,
    
    /// <summary>
    /// Output to a Bluetooth Low Energy (LE) peripheral.
    /// </summary>
    BluetoothLE = 6,
    
    /// <summary>
    /// Output to a Bluetooth A2DP device.
    /// </summary>
    BluetoothA2DP = 7,
    
    //---Input---
    
    /// <summary>
    /// Line-level input from the dock connector.
    /// </summary>
    LineIn = 8,
    
    /// <summary>
    /// The built-in microphone on a device.
    /// </summary>
    BuiltInMic = 9,
    
    /// <summary>
    /// A microphone that is built-in to a wired headset.
    /// </summary>
    HeadsetMic = 10,
    
    //---Input-Output---
    
    /// <summary>
    /// Input or output on a Bluetooth Hands-Free Profile device.
    /// </summary>
    BluetoothHFP = 11,
    
    /// <summary>
    /// Input or output on a Universal Serial Bus device.
    /// </summary>
    UsbAudio = 12,
    
    /// <summary>
    /// Input or output via Car Audio.
    /// </summary>
    CarAudio = 13,
} IosAudioPortType;

//Interop array with C#, by having C# allocate large enough empty array and this one write to it..
+(void) GetDeviceAudioInformation: (double*)interopArray OutputDeviceEnumArray:(int*) outputDeviceEnumArray
{
    //Various shared audio session properties..
    AVAudioSession* sharedInstance = [AVAudioSession sharedInstance];
    interopArray[0] = [sharedInstance outputLatency];
    interopArray[1] = [sharedInstance sampleRate];
    interopArray[2] = [sharedInstance preferredSampleRate];
    interopArray[3] = [sharedInstance IOBufferDuration];
    interopArray[4] = [sharedInstance preferredIOBufferDuration];
    
    //Output devices
    AVAudioSessionRouteDescription *routeDescription = [sharedInstance currentRoute];
    NSArray<AVAudioSessionPortDescription*> *outPorts = routeDescription.outputs;
    int i = 0;
    for (AVAudioSessionPortDescription *port in outPorts)
    {
        IosAudioPortType iapt;
        
        if([port.portType isEqualToString:AVAudioSessionPortLineOut]) iapt = LineOut;
        else if([port.portType isEqualToString:AVAudioSessionPortHeadphones]) iapt = Headphones;
        else if([port.portType isEqualToString:AVAudioSessionPortBuiltInReceiver]) iapt = BuiltInReceiver;
        else if([port.portType isEqualToString:AVAudioSessionPortBuiltInSpeaker]) iapt = BuiltInSpeaker;
        else if([port.portType isEqualToString:AVAudioSessionPortHDMI]) iapt = HDMI;
        else if([port.portType isEqualToString:AVAudioSessionPortAirPlay]) iapt = AirPlay;
        else if([port.portType isEqualToString:AVAudioSessionPortBluetoothLE]) iapt = BluetoothLE;
        else if([port.portType isEqualToString:AVAudioSessionPortBluetoothA2DP]) iapt = BluetoothA2DP;
        else if([port.portType isEqualToString:AVAudioSessionPortLineIn]) iapt = LineIn;
        else if([port.portType isEqualToString:AVAudioSessionPortBuiltInMic]) iapt = BuiltInMic;
        else if([port.portType isEqualToString:AVAudioSessionPortHeadsetMic]) iapt = HeadsetMic;
        else if([port.portType isEqualToString:AVAudioSessionPortBluetoothHFP]) iapt = BluetoothHFP;
        else if([port.portType isEqualToString:AVAudioSessionPortUSBAudio]) iapt = UsbAudio;
        else if([port.portType isEqualToString:AVAudioSessionPortCarAudio]) iapt = CarAudio;
        
        outputDeviceEnumArray[i] = iapt;
        
        i++;
    }
}

@end

extern "C" {
    
    int _Initialize() {
        return [NativeAudio Initialize];
    }
    
    void _GetDeviceAudioInformation(double* interopArray, int* outputDeviceEnumArray)
    {
        return [NativeAudio GetDeviceAudioInformation:interopArray OutputDeviceEnumArray: outputDeviceEnumArray];
    }
    
    int _SendByteArray(char* byteArrayInput, int byteSize, int channels, int samplingRate, int resamplingQuality)
    {
        return [NativeAudio SendByteArray:byteArrayInput audioSize:byteSize channels:channels samplingRate:samplingRate resamplingQuality: resamplingQuality];
    }
    
    int _LoadAudio(char* soundUrl, int resamplingQuality) {
        return [NativeAudio LoadAudio:soundUrl resamplingQuality: resamplingQuality];
    }
    
    void _PrepareAudio(int bufferIndex, int nativeSourceIndex) {
        [NativeAudio PrepareAudio:bufferIndex IntoNativeSourceIndex:nativeSourceIndex];
    }
    
    void _PlayAudioWithNativeSourceIndex(int nativeSourceIndex, NativeAudioPlayAdjustment playAdjustment) {
        [NativeAudio PlayAudioWithNativeSourceIndex:nativeSourceIndex Adjustment: playAdjustment];
    }
    
    float _LengthByAudioBuffer(int bufferIndex) {
        return [NativeAudio LengthByAudioBuffer: bufferIndex];
    }
    
    void _StopAudio(int nativeSourceIndex) {
        [NativeAudio StopAudio:nativeSourceIndex];
    }
    
    void _SetVolume(int nativeSourceIndex, float volume){
        [NativeAudio SetVolume:volume OnNativeSourceIndex:nativeSourceIndex];
    }
    
    void _SetPan(int nativeSourceIndex, float pan){
        [NativeAudio SetPan:pan OnNativeSourceIndex:nativeSourceIndex];
    }
    
    float _GetPlaybackTime(int nativeSourceIndex){
        return [NativeAudio GetPlaybackTimeOfNativeSourceIndex: nativeSourceIndex];
    }
    
    void _SetPlaybackTime(int nativeSourceIndex, float offsetSeconds){
        [NativeAudio SetPlaybackTimeOfNativeSourceIndex:nativeSourceIndex Offset: offsetSeconds];
    }
    
    void _Pause(int nativeSourceIndex)
    {
        [NativeAudio Pause: nativeSourceIndex];
    }
    
    void _Resume(int nativeSourceIndex)
    {
        [NativeAudio Resume: nativeSourceIndex];
    }
    
    void _UnloadAudio(int index) {
        [NativeAudio UnloadAudio:index];
    }

    int _GetNativeSource(int index)
    {
        return [NativeAudio GetNativeSource:index];
    }
}
