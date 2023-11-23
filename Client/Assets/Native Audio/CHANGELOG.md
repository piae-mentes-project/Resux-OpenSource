# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

# [7.0.0] - 2022-01-15

Despite the front SemVer number moving up, this update is minor (but technically breaking for some users). Asset Store is now [moving along with editor's LTS cycle](https://forum.unity.com/threads/asset-store-following-unity-editor-lts-cycle-final-call-for-2018-x-please-update-your-assets.1014994), only accepting new submission on the lowest LTS and accepting updates from the most recent fallen off LTS.

2018 LTS is already going away, and next in queue is 2019 LTS. I had also evaluated that 2019 LTS [is the best version](https://gametorrahod.com/asset-store-upm-package-lts/) for publishing packages right now. Therefore, Native Audio is moving to minimum 2019 LTS in this version.

## Changed

- Refactored all XML code documentation. Notably the correct usage of `<para></para>` to give you paragraphs instead of wall of texts, bullet points using `<list><item></item></list>` to render correctly, and inline code wrapped with `<c></c>`
- [Android] `NativeAudio.aar` rebuilt with `compileSdkVersion 29`, `targetSdkVersion 29`. (Up from 26, right now Google Play requires 29 or higher for app submission, though this is not an app.)
- [iOS] `libsamplerate` folder that is inside `Plugins/iOS` folder is modified from the original, now all executables included originally are removed. This is becase the new Asset Store detects executables and automatically rejects the submission.

## Fixed

- [Android] Setting volume to exactly 0 was not working because it was thrown into `log10(volume)` function. Now volume 0 is really 0.
- [iOS] Remove unused function in `NativeAudio.h`
- [iOS] Fixed wrong return type of `PrepareAudio` in `NativeAudio.mm`.
- [iOS] Fixed wrong returning value of `GetNativeSource` in `NativeAudio.mm`.

# [6.0.0] - 2020-05-01

Major SemVer version bump means breaking, incompatible change.

## `InitializationOptions.androidMinimumBufferSize` changed to `androidBufferSize`

**This is not just a rename!**

The meaning of "minimum" is that you may not get exactly the number you want. That is what it was.

Previously there exist a filter that clamp the buffer size to device's native buffer size if it was too low. It cause problem when some phone indicate its own buffer size for too high and now developer has no way to push the buffer size to smaller value that could result in better latency.

The filter also optimize the number that is higher that native buffer size to become the next multiple of native buffer size to reduce jitter. (e.g. native buffer size is 256 but you want 258 for some reason, the filter change it to 512.)

Now, it honors any positive number you give it and initialize with exactly that buffer size. Note that a buffer size too low now potentially crash the application. The change is to address some phone which could actually push to lower number, because it overestimate its own safe buffer size.

One magic remains, which is if you give it negative value it will turn that into device's native buffer size as negative size does not make sense. This make the previous -1 value still behave the same way. What changed is that if you give it absurdly low number like 1 now it really use 1 as a buffer size. (The audio will be completely destroyed or crash in that case.) Also odd number get +1 into the next nearest even number due to technical reasons.

The code error after upgrading to this version should guide you to everywhere in the game that you use custom buffer size.

## Added `InitializationOptions.OptimizeBufferSize(bufferSize)`

That filter is now moved to a static helper method : `InitializationOptions.OptimizeBufferSize(bufferSize)` for you to **manually use**, then pass the returned number to initialize to get the old behaviour.

## Added `NativeAudio.GetNativeSourceCount()`

This is useful in a case like you want to completely stop all audio played with Native Audio before unloading audio to prevent crash where the play head now play unloaded memory. Therefore you need to get **all** native sources and stop each one. With this count you could do so in a `for` loop.

On Android, it returns as many as you actually got from initialization. (e.g. if you requested 999 and actually get 15, then this returns 15) For iOS it is currently fixed to 15 as that is how OpenAL works. It gives you a set amount of sources.

## Fixed

- Few samples of the beginning of audio were missing in each play equal to how large your buffer size is. This is fixed now. (off-by-one error, whoops..)

# [5.0.0] - 2019-12-01

Major SemVer version bump means breaking, incompatible change.

This release contains a massive syntactic change, requiring some line-to-line code modification. I discovered as I am writing an entire website again that even I got confused with my own API. It is kind of enlightening, now the new API goes along with all explanation in the website making it more welcoming to new users and make more sense in the code editor.

However functionally stays mostly the same. If you don't have time it is not urgent that you have to update. There is no important bug fix here. Performance improvement is just very slightly better.

It is highly recommended to not import over the old one, but remove the old one completely before importing this version. Please make sure you got enough time to do a code replace before updating to this version.

## 5.0.0 API Changes

Here's a big section of syntactic API changes. Read it carefully for diff with the previous version, or if you prefer, re-read the [entirely new website](https://exceed7.com/native-audio/) that is now written according to this new API.

#### `NativeAudio` is now a `static` class

As it should have been from the beginning. This class is full of `static` methods for interfacing with native side. An instance of it make absolutely no sense. I was a noob programmer back then.

#### `NativeAudioController` renamed to `NativeSource`

A naming change with big implication. It represent one native source that was used in a play. "NativeAudioController" sounds like it control the entire plugin more that an intended meaning.

Except this now make sense even before a play. You can have a hold of this variable and keep it (and even name it!) by a newly added method `NativeAudio.GetNativeSource(index)` just like you would hold an `AudioSource`, without first playing anything to get it.

It is now not a "controller" and you can think of it like `AudioSource`. Meaning that previously you can only `Stop` `Pause` `Resume` them and so on ("controlling" them), now you can expect that `Play` is a part of it just like `AudioSource`. This leads to the next point.

#### How you play an audio completely changed

**All previous users have to take action for this change.**

To play any audio, now you have 2 ways :

- New `NativeSource.Play(nativeAudioPointer)` instance method : I already have a native source target I want, play on this source. To get a `NativeSource` of index that you want, use the new `NativeAudio.GetNativeSource(index)`. You no longer specify a target as an integer in `PlayOption`. That was very awkward. You can cache and reuse a `NativeSource` just like `AudioSource`.
- Use `NativeAudio.GetNativeSourceAuto()` in order to play "without caring". This "Auto" means the round-robin behaviour, but this time you just get back a desired `NativeSource` and not a play yet. Then you use `Play` on it. The `GetNativeSourceAuto` naming allows a "different auto" to be implemented as an overload.

#### `INativeSourceSelector` new `interface` added

This is that "different auto" usable with `GetNativeSourceAuto(INativeSourceSelector)`.

#### `nativeAudioPointer.Play` instance method removed

**All previous users have to take action for this change.**

Continued from the previous point, now you put `NativeAudioPointer` in `NativeSource.Play` instance method. This is just a syntax change, the function didn't change. You can fix this line by line without affecting anything else.

If previously :

```csharp
myNativeAudioPointer.Play();
```

Fix to :

```csharp
NativeAudio.GetNativeSourceAuto().Play(myNativeAudioPointer);
```

If previously :

```csharp
myNativeAudioPointer.Play(playOptionWithExplicitSourceTarget);
```

Fix to :

```csharp
NativeAudio.GetNativeSource(explicitSourceIndex).Play(myNativeAudioPointer, playOption);
```

This is now more understandable and more representative at what happened at the native side. The old `Play` that is sticking on an instance of `NativeAudioPointer` didn't make much sense as technically this pointer do not have an ability to play. **It's the native source that plays using this pointer**.

Plus the documentation could be made less confusing that `Initialize` `Load` `Play` `Dispose` are all on the same "thing" (`NativeAudio`).

Now only freeing audio memory stays on the `NativeAudioPointer` (`Unload()`) as I think it is more intuitive that an audio can dispose itself.

#### `PlayOption.audioPlayerIndex` removed

The previous points explain that now the way to explicitly target a native source is to get a desired `NativeSource` variable and `.Play` on it, we no longer awkwardly use an integer target in the `PlayOption`.

You can get a desired `NativeSource` with the new `NativeAudio.GetNativeSource` or `NativeAudio.GetNativeSourceAuto`. 

`PlayOption` is now strictly things like volume, pan, etc. not related to more technical aspects like which native source to use.

#### "Prepare" concept completely changed

An act of preparation is to pre-assign **audio to a source** without playing so the play could be faster if possible.

Therefore this "prepared" status should be tied to a source, not to the audio data/pointer. Therefore `NativeAudioPointer.Prepared` status is now removed. The same reason as the move of `Play` out of pointer. We told any native source to prepare about this pointer, not that we told the pointer to prepare itself. That is impossible and make no sense. Pointer is just a data, it can't prepare anything. Rather, the native sources has to prepare.

`nativeAudioPointer.Prepare` instance method is now moved to `NativeSource.Prepare(nativeAudioPointer)` instance method.

A way of "using the preparation" is also changed. Previously the API expect you to use the same pointer in the "next" play and it will be sort of automatically faster. That was too magical and full of bugs. If you wait too long other audio uses that source then your preparation is now invalid. If you don't play with that pointer then the prepared status could never be cleared out, etc.

Now we have a new explicit method `NativeSource.PlayPrepared()`. The key point to note is that it **has no audio pointer argument**. Imagine releasing a charged attack, it release whatever it was charged, however it can't be helped if the audio changed to something else already. This no argument design make it clearer that the result depends on prepared audio and an internal status.

#### `NativeSource` (formerly `NativeAudioController`) is now a `struct` instead of `class`

This object is potentially generated with `NativeAudio.GetNativeSourceAuto` on *every play*. Previously `NativeAudioController` is also returned on *every play*. So this is a general performance boost as well. Being a `class` means we are increasing works for the GC rapidly as playing audio is a common event.

A stack allocated `struct` is better because if you choose not to care then its memory is cleanly freed after the function's scope. It is just a number, there is nothing that would need a `class`'s ability at all. 

#### Native API naming changes for both iOS and Android

Regular user won't be affected, but over the years there are many who attempted to connect things up at entirely native side, not wanting to touch C#. If you are one, various weird naming were fixed to be more consistent. You have to rename accordingly.

#### Other API naming changes

All documentations in code XML and website now refer to the thing that you get at initialize waiting to play your loaded audio as **native source**. It is named like this to parallel Unity's `AudioSource` but instead living at native side. The "audio player" and "track" confusing wording that was meant to be the same thing has been cleaned up to "native source" everywhere, both documentation and the method names.

- `PlayOptions.trackLoop` changed to `PlayOptions.sourceLoop`. The "track" wording is deprecated. (It was like that because on Android it is called `AudioTrack` in C++)
- `NativeSource.TrackPause()` and `NativeSource.TrackResume()` is now just `Pause()` and `Resume()` as it is already clear we are doing it on the "native source", as opposed to previously bad name `NativeAudioController`.
- `NativeSource.InstanceIndex` changed to `NativeSource.Index`. The "index" wording is used to say "which" native source you would like to use now. That "instance" looks alien.
- `PlayOptions` moved out of `NativeAudio` nesting, into `NativeSource` nesting instead, since it is now a thing that is used with the source. `InitializationOptions` and `LoadOptions` remain nested in `NativeAudio` class, since it is a thing that used on Native Audio overall.

Non "native source" related changes : 

- `NativeAudio.OnSupportedPlatforms()` changed to a property instead : `NativeAudio.OnSupportedPlatforms`.

## Other changes

#### Unity Package Manager compatible Samples

`Demo` folder is now renamed to `Samples~`, which is now hidden from Unity importer *until you need it*. Various demo audio inside now properly **not** imported to your game, saving you import time on switching platforms. In this [new convention](https://forum.unity.com/threads/samples-in-packages-manual-setup.623080/), samples are imported on demand to your project by a button in the Package Manager instead of being there from the start. You can also just copy to your project out of `~` suffixed folder. 

#### Folder structure in Unity's UPM convention

- `Managed` folder renamed to `Runtime`. `.asmdef` moved into the `Runtime` folder as well.
- `Extras` folder merged into the new `Samples~`, you can get its content by sample importing or just copy it out.
- `Documentation~` hidden folder added to the package. It contains a Markdown version of the official website https://exceed7.com/native-audio.
- `HowToUse` offline zipped website documentation removed in favor of Markdown documentation in `Documentation~` folder.
- `Documentation~` and `Samples~` included in `.zip` form until Asset Store support publishing unimported folder. Please unzip it manually when you need them. (Either zipped or unzipped to `___~`, the content won't be unnecessarily imported to your game.)

#### Discontinued support for 2017.1.5f1.

Now the lowest supported version is 2017.4.34f1. (The lowest possible LTS in Unity Hub.)

Trivia : 2017.1.5f1 didn't have Assembly Definition Files, supporting that version make it very difficult to utilize `internal` keyword together with `[InternalsVisibleTo]` as it doesn't work with `Editor` magic folder.

#### Others

- New website design : https://exceed7.com/native-audio. It is a big deal since now the website linked to `Documentation~` hidden folder in this package rather than having to handcraft it separately. Even this `CHANGELOG.md` turns into a webpage, along with API documentation that is the same as code documentation in this package. Now you can read documentation in their Markdown form in `Documentation~` or in the website. It will also reduce my maintenance burden, things in the web will stay up to date with the package 100%. Awesome!
- The web has better explanation why audio could get faster.
- Rewrite many method's code documentation. (Also reflected in the API page in the web.)

# [4.6.0] - 2019-10-01

## Changed

### Dropped support for x86 architecture in the prebuilt AAR.

Due to recent Google Play policy change to accept only 64-bit build and Unity deprecating x86 build (and you cannot even submit a build with x86 lingering in there if you opt in to Google's new Android App Bundle), I have rebuild the AAR with only `armeabi-v7a` (32-bit) and `arm64-v8a` (64-bit).

I had received a report that some phone mistakenly pick x86 in the AAR previously and crash on start even though it should have picked `arm64-v8a`, so the best way now that Unity is also removing x86, is to just remove x86 from the AAR.

If you are using pre 2019.2 Unity, don't check x86 in the Android build configuration. Doing so will only drop support for about 2 devices in the world according to Google Play console. No one made x86 devices to the market anymore.

## Fixed

- [Android] The AAR is compiled with `targetSdkVersion` 27 instead of previously 28, to support developers that use Android SDK of lower version with Unity.
- [Android] The project that compiled the AAR is upgraded to Android Gradle plugin version 3.5.0 and Gradle Version 5.4.1.

# [4.5.0] - 2019-08-15

## Added

- Better performance of OpenSL ES double buffering callback function by removing all `if`, replaced by inline conditionals. Compiled assembly could be better with inline conditionals since it may avoid costly branch prediction. This callback is a very hot function as it is being called on every little bits of audio buffer sends out your speaker, so potentially it could improve latency. (theoretically)
- Added some explanations why `nativeAudioPointer.Unload()` is unsafe and could cause segmentation fault in the code documentation. You have to ensure no tracks are playing before you unload. It is by design.

## Fixed

- The multi-track demo scene now wait 0.5s after disposing before re-initializing 4 native sources, to fix throttling time problem which cause you to not get back fast native tracks you just released.

# [4.4.0] - 2019-06-10

## Added

- Demo scenes now has a button that could jump to the next one. (You have to add them all in the build.)
- More kinds of demo scenes to test out more situations. Like stress test and multiple audio tracks test. You could use a scene that is similar to your game, to confirm a problem before submitting a bug report to me, for example.

## Changed

- `NativeAudio.Intialize` in editor now throws readable error rather than cryptic error about native call failure.

## Fixed

- [iOS] Phone call was cutting off Native Audio and it didn't initalize back. It is now properly reinitialized.
- [Android] Added a catch when the device somehow returns native sampling rate or buffer size number that is not parsable by `Integer.parseInt` at Java. I don't know the cause of invalid value yet and how many devices do it (only one user reported this to me so far, and for only 1 device, and I also don't know what number that looks like which Java couldn't parse), but it will now be defaulted to 44100 Hz + buffer size 256 in the case that `NumberFormatException` occurs. When this happen, `adb logcat` will print something like "Received non-parsable ..." and if possible please report the device that do this to me. Thank you.

# [4.3.0] - 2019-03-30

## Added

- [iOS] Now `NativeAudio.GetDeviceAudioInformation` returns the following iOS-specific information : `outputLatency`, `sampleRate`, `preferredSampleRate`, `ioBufferDuration`, `preferredIOBufferDuration`. These are of the shared `AVAudioSession` singleton, and are shared with Unity not just for Native Audio.

For reference, I tried varying Project Settings > Audio options and this is the `ioBufferDuration` :

Best Latency     : 0.0106666665524244
Good Latency     : 0.0213333331048489
Best Performance : 0.0426666662096977

## Fixed

- [Android] Fixed minimizing and come back would multiply the number of restored audio sources by 2x of number of soures before minimizing every time.
- [Android] Fixed native source destroy and restore mistakenly when the app loses focus e.g. accessing Google Play Game Center as a floating window mid-game and crash. The source are now properly only destroyed and restored on the app's minimize and maximize.
- [Android] Fixed minimizing and maximizing while in uninitialized state, which had been in initialized state before, that it cause Native Audio to initialize again on coming back. Disposing now correctly remove the lifecycle callback along with disposing the native audio sources.
- [Android] Fix `BuildConfig.java` mistakenly included in the .AAR that it cause DEX duplication on building with your game on Gradle. The Android Studio project is also updated to build without `BuildConfig.java` now.
- [iOS] Fix `NativeAudio.GetDeviceAudioInformation` only returns current **input** audio devices. Now it only returns current **output** audio devices.

# [4.2.1] - 2019-03-26

## Fixed

- [Android] Fixed an error where `NativeAudio.GetDeviceAudioInformation` throws JNI exception on Android lower than API 23 (6.0/Marshmallow) because that API couldn't check for active output devices. It is now properly set to `null` on unsupported API level.

# [4.2.0] - 2019-03-25

## Changed

### [Android] Now dispose all native sources on minimize, and restore on coming back

Previously the allocated audio sources on Android will not be freed when minimize the app. (The Unity ones do freed and request a new one on coming back) This make it possible for audio played with Native Audio to play while minimizing the app, and also to not spend time disposing and allocating sources again.

However this is not good since it adds "wake lock" to your game. With `adb shell dumpsys power` while your game is minimized after using Native Audio you will see something like ` PARTIAL_WAKE_LOCK 'AudioMix' ACQ=-27s586ms (uid=1041 ws=WorkSource{10331})`. Meaning that the OS have to keep the audio mix alive all the time. Not to mention most games do not really want this behaviour.

Most gamers I saw also minimized the game and sometimes forgot to close them off. This cause not only battery drain when there is a wake lock active, but also when the lock turns into `LONG` state it will show up as a warning in Google Play Store, as it could detect that an app has a [Stuck partial wake lock](https://developer.android.com/topic/performance/vitals/wakelock) or not.

So in this version, on initialize the native side will remember your request's spec. On minimize it will dispose all the sources (and in turn stopping them). On coming back it will reinitialize with the same spec thanks to reinitialization possible from version 4.1.0.

### [Android] New initialization option `preserveOnMinimize`

When setting this to `true` it would behave like earlier version where it wouldn't release the native sources on minimize/sleep. Be careful about your wake locks.

### [Android] NativeAudio.Dispose

This method allows you to just dispose the sources Native Audio is using without intialize back. This is to be used together with the new `preserveOnMinimize` option, so you have more control over your app's wake lock.

Disposing has nothing to do about loaded audio. It just dispose the native sources. To unload audio you still need the `NativeAudioPointer` you kept.

### No longer automatically initialize on loading audio

Previously if your forgot to `NativeAudio.Initialize` and go straight to `NativeAudio.Load`, it will initialize with default options for you. Together with various initialization control methods added in this release it would be the best to hand the initialization control completely to you.

### No longer able to load, use any NativeAudioPointer, or NativeAudioController while not in initialized state

Various exception throws are added if you attempt to use Native Audio while not initialized, or even initialized but suddenly goes uninitialize due to the new `NativeAudio.Dispose`. Now that Native Audio can went back to uninitialized at will, I have defined additional rules.

Even though `NativeAudio.Load` has nothing to do with audio playing, you are not allowed to load while in uninitialized state. The reason is because each load is using the buffer size you tell Native Audio at `NativeAudio.Initialize` to optimize a buffer and reduce as much jitter as possible.

Then what if you initialize, then load, then reinitialize with a new buffer size, then play old audio loaded previously? In fact they are **still working** but not as jitterless as audio loaded under active buffer size.

`NativeAudioPointer.Play` cannot be used while not initialized obviously. Interestingly you are still allowed to unload `NativeAudioPointer`, that's about the only thing you are allowed to do.

`NativeAudioController` connects directly to sources at native side, so in uninitialized state they had been all destroyed and is unusable.

All these checks are purely at C# managed side. If you are hacking Native Audio and use the native side method you will get SIGSEGV when trying to do something while uninitialized.

## Fixed

- [iOS] Fixed a bug where playing long audio fast enough, that the automatically selected source loops over to the same source that still hadn't finished playing yet fails silently to assign a new audio source and ended up replaying the unfinished source.
- Added "Dispose" button to the demo scene for you to try out `NativeAudio.Dispose`.

# [4.1.0] - 2019-03-20

## Added

### [Android] Reinitialization

Now it is possible to "reinitialize" by calling `NativeAudio.Initialize()` **again** on Android. This allows you to fix up the `InitializationOption` that you got in wrong the first time. I have some report that for Chinese phones like Huawei Mate 20, P20 or some MeiZu phones, turns out the phone's recommended buffer size was too low to be usable. (Also an issue with normal Unity audio)

On reinitialization all native sources will be disposed. So you could use a lower number than intitially allocated if you wish.

Reinitialization allows you to implement a slider in the game's option screen to manually adjust the buffer size until it is usable, for example. In option menu you may have an advanced section saying "if you experience audio problem, you may try increasing the buffer size at the cost of larger latency." ...or something.

The reinitialization is quite costly so I don't recommend doing it rapidly when the slider is moving. Instead maybe an apply button is more appropriate.

Note that choosing a new arbitrary `androidMinimumBufferSize` will **not** get you that exact size, but still be modified to be a multiple of device's optimal buffer size to reduce jitter. (The same as before)

### GetDeviceAudioInformation.audioDevices added

It is an array of `enum` specifying types of device currently active. I have heard that Native Audio may produce strange glitch on Bluetooth devices, or maybe on other unusual devices. Ideally I would like Native Audio to work everywhere, but in emergency you can check on this array and maybe turn off Native Audio based on type of devices your user is using.

Android and iOS has different set of output devices ported directly from respective native side. The `DeviceAudioInformation` has preprocessor directive to switch its available `enum` depending on platform.

- C# XML code documentation now utilizes more XML tags to link up method references.
- Demo scene is updated to be able to reinitialize with any buffer size. Specifiable with a slider.

## Changed

- [Android] MIPS and MIPS64 variant has been removed from Android's built AAR since it was causing compile problem in Unity.

## Fixed

- Fixes bug in the old `StreamingAssets` loading on Android where it throws : "System.Exception: JNI: Unknown signature for type 'E7.Native.NativeAudio+LoadOptions+ResamplingQuality'" because I forgot that the native side was waiting with `int` signature, while at managed side I sent the `enum` it is now casted to `int`. (But still I recommend using the new `AudioClip` way.)
- [iOS] Fixed a bug on iOS where if you play a certain audio, only when rapidly over a certain frequency, every 16th play would have its right channel turned off and logs something like this on Xcode : `ProductName[239:4538] AUBase.cpp:832:DispatchSetProperty:  ca_require: ValidFormat(inScope, inElement, newDesc) InvalidFormat`. I don't know if this is OpenAL's bug or something, the entire internet do not have any say about it, and that line itself is literally a part of code in OpenAL's source code and not indicating any error, but it seems like reducing the total available source to 15 instead of 16 fixed the issue. (Seems like the 32th source of OpenAL where our 16th source ended up using as its right channel is buggy... wtf)
- Added GC pinning on the `AudioClip` way of loading for safety, to prevent C# GC from moving the content while native side is reading it.
- Taken care of all compilation warnings in the source code.

# [4.0.0] - 2018-12-24

## Added

### [All Platforms] New load API : `NativeAudio.Load(AudioClip)`

You are now freed from `StreamingAssets` folder, because you can give data to NativeAudio via Unity-loaded `AudioClip`.

Here's how loading this way works, it is quite costly but convenient nonetheless :

- It uses `audioClip.GetData` to get a float array of PCM data.
- That float array is converted to a byte array which represent 16-bit per sample PCM audio.
- The byte array is sent to native side. NativeAudio **copy** those bytes and keep at native side. You are then safe to release the bytes at Unity side without affecting native data.
- Thus it definitely takes more time than the old `StreamingAssets` folder way. Your game might hiccups a bit since the copy is synchronous. Do this in a loading scene.

This is now the recommeded way of loading audio, it allows a platform like PC which Native Audio does not support to use the same imported audio file as Android and iOS. Also for the tech-savvy you can use the newest Addressables Asset System to load audio from anywhere (local or remote) and use it with Native Audio once you get a hold of that as an `AudioClip`.

Hard requirements : 

- Load type **MUST be Decompress On Load** so Native Audio could read raw PCM byte array from your compressed audio.
- If you use Load In Background, you must call `audioClip.LoadAudioData()` beforehand and ensure that `audioClip.loadState` is `AudioDataLoadState.Loaded` before calling `NativeAudio.Load`. Otherwise it would throw an exception. If you are not using Load In Background but also not using Preload Audio Data, Native Audio can load for you if not yet loaded.
- Must not be ambisonic.

In the Unity's importer, it works with all compression formats, force to mono, overriding to any sample rate, and quality slider.

The old `NativeAudio.Load(string audioPath)` is now documented as an advanced use method. You should not require it anymore in most cases.

### [All Platforms] OGG support added via `NativeAudio.Load(AudioClip)`

From the previous point, being able to send data from Unity meaning that we can now use OGG. I don't even have to write my own native OGG decoder!

The load type must be **Decompress on Load** to enable decompressed raw PCM data to be read before sending to Native Audio. This means on the moment you load, it will consume full PCM data in Unity on the read **and** also full PCM data again in native side, resulting in double uncompressed memory cost. You can call `audioClip.UnloadAudioData` afterwards to free up memory of managed side leaving just the uncompressed native memory.

OGG support is not implemented for the old `NativeAudio.Load(string audioPath)`. An error has been added to throw when you use a string path with ".ogg" to prevent misuse.

### [iOS] Resampler added, but not enabled yet

I have added `libsamplerate` integration to the native side but not activate it yet.

Now you can load an audio of any sampling rate. Currently I don't have an information what is the best sampling rate (latency-wise) for each iOS device, now I left the audio alone at imported rate.

Combined with the previous points, you are free to use any sampling rate override import settings specified in Unity.

### [All Platforms] Mono support added

- When you loads a 1 channel audio, it will be duplicated into 2 channels (stereo) in the memory. Mono saves space only on the device and not in-memory.
- Combined with the previous points, you are free to use the `Force To Mono` Unity importer checkbox. 

### [Android] NativeAudio.GetDeviceAudioInformation()

It returns audio feature information of an Android phone. [Superpowered is hosting a nice database of these information of various phones.](https://superpowered.com/latency).

Native Audio is already instantiating a good Audio Track based on these information, but you could use it in other way such as enforing your Unity DSP buffer size to be in line with the phone, etc. There is a case that Unity's "Best Latency" results in a buffer size guess that is too low it made Unity-played audio slow down and glitches out.

## Changed

### `LoadOptions.androidResamplingQuality` renamed to `LoadOptions.resamplingQuality`

Because now iOS can also resample your audio.

## Removed

### [EXPERIMENTAL] Native Audio Generator removed

It just here for 1 version but now that the recommended way is to load via Unity's importer this is not worth it to maintain anymore. (That's why I marked it as experimental!)

# [3.0.0] - 2018-11-01

## Added

### [All Platforms] Track's playhead manipulation methods added

- `NativeAudio.Play(playOptions)` : Able to specify play offset in seconds in the `PlayOptions` argument.
- `NativeAudioController` : Added track-based pause, resume, get playback time, and set playback time even while the track is playing. Ways to pause and resume include using this track-based pause/resume, or use get playback time and store it for a new `Play(playOptions)` later and at the same time `Stop()` it immediately, if you fear that the track's audio content might be overwritten before you can resume.
- `NativeAudioPointer` : Added `Length` property. It contains a cached audio's length in seconds calculated after loading.

### [All Platforms] Track Looping

A new `PlayOptions` applies a looping state on the TRACK. It means that if some newer sound decided to use that track to play, that looping sound is immediately stopped.

To protect the looping sound, you likely have to plan your track number usage manually with `PlayOptions.audioPlayerIndex`.

- If you pause a looping track, it will resume in a looping state.
- `nativeAudioController.GetPlaybackTime()` on a looping track will returns a playback time that resets every loop, not an accumulated playback time over multiple loops.

### [iOS] Specifying a track index

Previously only Android can do it. Now you can specify index 0 ~ 15 on iOS to use precisely which track for your audio. It is especially important for the new looping function.

### [EXPERIMENTAL] Native Audio Generator

When you have tons of sound in `StreamingAssets` folder it is getting difficult to manage string paths to load them.

The "Native Audio Generator" will use a script generation to create a static access point like this : `NativeAudioLibrary.Action.Attack`, this is of a new type `NativeAudioObject` which manages the usual `NativeAudioPointer` inside. You can call `.Play()` on it directly among other things. You even get a neat per-sound mixer in your `Resources` folder which will be applied to the `.Play()` via `NativeAudioObject` automatically.

Use `Assets > Native Audio > Generate or update NativeAudioLibrary` menu, then you can point the pop-up dialog to any folder inside your `StreamingAssets` folder. It must contain one more layer of folder as a group name before finally arriving at the audio files. Try this on the `StreamingAssets` folder example that comes with the package.

This is still not documented anywhere in the website yet, but I think it is quite ready for use now. EXPERIMENTAL means it might be removed in the future if I found it is not good enough.

## Removed

### `PlayAdjustment` inside the `PlayOptions` is no more.

Having 2 layers of configuration is not a good API design, but initially I did that because we need a struct for interop and we need a class for its default value ability.

I decided to make it 1 layer. The entire `PlayOptions` is now used to interop with the native side.

Everything is moved to the `PlayOptions`, and also `PlayOptions` is now a struct. Previously the `PlayAdjustment` inside is the struct. Not a class anymore, now to get the default `PlayOptions` you have to use `PlayOptions.defaultOptions` then you can modify things from there. If you use `new PlayOptions()` the default value of the struct is not a good one. (For example volume's default is supposed to be 1, not int-default 0)

# [2.1.0] - 2018-09-12

## Added

### [IOS] 2D Panning

The backend `OpenAL` of iOS is a 3D positional audio engine. 2D panning is emulated by deinterleaving a stereo source audio into 2 mono sources, then adjust the distance from the listener so that it sounds like 2D panning.

### [ALL PLATFORMS] Play Adjustment

There is a new member `playAdjustment` in `PlayOptions` that you can use on each `nativeAudioPointer.Play()`. You can adjust volume and pan right away BEFORE play. This is because I discorvered on iOS it is too late to adjust volume immediately after play with `NativeAudioController` without hearing the full volume briefly.

## Fixed

- Previously the Android panning that is supposed to work had no effect. Now it works alongside with the new iOS 2D panning.

# [2.0.0] - 2018-09-08

## Added

### [Android] Big migration from Java-based AudioTrack to OpenSL ES

Unlike Java AudioTrack, (which built on top of OpenSL ES with similar latency from my test) OpenSL ES is one of the officially mentioned "high performance audio" way of playing audio right here. It will be awesome. And being in C language part Unity can invoke method via extern as opposed to via AndroidJavaClass like what we have previously. (speed!) Only audio loading has to go through Java because the code have to look for StramingAssets folder or OBB packages, but loading is only a one-time thing so that's fine.

I am proud to present that unlike v1.0, Native Audio v2.0 follows everything the official Android high-performance audio guidelines specified. For details of everything I did for this new "back end" of Native Audio, please [read this research](https://gametorrahod.com/androids-native-audio-primer-for-unity-developers-65acf66dd124). 

### [Android] Resampler

Additionally I will go as far as resampling the audio file on the fly (we don't know which device the player will use, but we can only prepare 1 sampling rate of audio practically) to match each device differing native sampling rate (today it is mainly either 44100Hz or 48000Hz) so that the special "fast path" audio is enabled. The previous version does not enable fast path if device is not 44100Hz native because we hard-fixed everything to 44100Hz. This will be awesome for any music games out there. (But it adds some load time if a resampling is required, it is the price to pay)

About resampling quality do not worry, as instead of writing my own which would be super slow and sounds bad I will incorporate the impressive libsamplerate (Secret Rabbit Code) http://www.mega-nerd.com/SRC/ and it has a very permissive BSD license that just require you to put some attributions, not to open source your game or anything. You are required to do your part in the open source software intiative.

### [Android] Double buffering

The previous version not only it use Java it also push one big chunk of audio in the buffer. In this version, with double buffering technique we put just a small bit of audio and we are ready to play way faster. While this one is playing the next bit will be prepared on the other buffer. It switch back and forth like this until we used up all the sameples. The size of this "audio bit" is set to be as small as possible.

### [Android] Native buffer size aligned zero-padded audio

Even more I will intentionally zero pad the audio buffer so that it is a multiple of "native buffer size" of each device further reducing jitter when pushing data to the output stream. High-end device has smaller native buffer size and require less zero-pad. Combined with double buffering mentioned earlier the play will ends exactly without any remainders buffer.

### [Android] Keep alive the audio track

Unlike naive implementation of playing a native audio on Android, Native Audio use a hack which keep the track constantly **playing silence** even if nothing is playing.

This is to counter the costly audio policy that triggers on a transition between play and stopped state on some badly programmed Android phone. It makes rapidly playing audio lags some phones badly.

Big thanks to PuzzledBoy for helping me investigating into this problem.

## Changed

### Volume/pan/etc. adjustments on sound moved to `NativeAudioController`, a new class type.

It is returned from the new `Play()`, previously returns nothing. Please use it in the line immediately after `Play()` to adjust volume instead of as an argument on `Play()`.

### Requiring an open source attribution (BSD) to `libsamplerate`

Native Audio is now powered by `libsamplerate`, required for the minimum latency without noticable compromise on audio quality!
Please visit the [how to use](http://exceed7.com/native-audio/how-to-use.html) page for details. 

### Initialize, Load, Play now has an option argument.

It provides various customization which you can read in the website or in code comment/Intellisense.

### Completely new Android underlying program : OpenSL ES and the AudioTrack customization.

Now it is crucial to know that Android requests 3 AudioTracks by default and you can change this with the initialization options.
Increasing this number increases concurrency but with consequence. Please visit [the homepage](http://exceed7.com/native_audio) and read carefully.

### Audio format requirement relaxed only on Android

In Android thanks to `libsamplerate` you can use any rate now, but it is not on iOS yet. For now, stick with **16-bit PCM stereo 44100Hz .wav** file.

# [1.0.0] - 2018-04-13

The first release.

# On Investigation

Consider this section a bonus. They are draft of things that came to my mind. It may not happen, so do not hope or expect them.

### Buffer underrun detector + auto fix

Currently the big problem on both normal Unity and Native Audio is that some phones (Mostly Chinese, Huawei Mate / P20, MeiZu) reports wrong optimal buffer size that the phone itself cannot handle and ended up craking audio. This is called buffer underrun. At the same time increasing buffer size may increase latency.

The next problem is could we detect buffer underrun occuring programmatically, not using our ears? I have been thinking of an "autocalibrator" where it test out (secretly, in silence) if the underrun occurs and adjust buffer size accordingly until that is fixed.

Each time of the adjustment Native Audio have to reinitialize all audio sources. This is quite an expensive operation.

### Faster Unity -> native audio memory copy

Coming with "using Unity's imported audio data" support in 4.0.0 is a costly memory copy. I would like to use `unsafe` or GC pinning in some ways to make the native side able to access memory in Unity without copy. Unsafe code will be wrapped in NativeAudio's unsafe assembly definition file if it is implemented.

### [iOS] Resampler

Similar to the current Android side, we will resampling all audio to match the device's preferred sampling rate so we can reduce the hardware's work as much as possible. It will also use the libsamplerate.

Currently we still require 44100Hz audio because even at Android side it is able to resample to any rate in iOS we still have a fixed sample rate player. When this feature arrives, we are finally able to use any sampling rate of the .wav file. Then from that point onwards it is recommended to go for 48000Hz since phones in the future is likely to use this rate, and with that those players will not have to spend time resampling + get the true quality audio file.

### [Android] AAudio support via Oboe

Big news! (https://www.youtube.com/watch?v=csfHAbr5ilI) 

It seems that "Oboe" is now out of developer preview. This is Google-made C++ audio library for Android that seems to do exactly what Native Audio wants to do.

Including the AAudio and OpenSL ES fallback thing I intended to do. Anyways, it needs to pass my latency comparison with the current way first and I will let you know the result when that's done. If it is equal or lower I will switch to Oboe implementation. Then we all will get automatic **AAudio** support. Wow!

### PC Support

PC is also known for outputting slow Unity audio. But I will have to learn much more about PC's native audio first.

### Samsung SAPA support

This rather obscure Samsung-only API apparently could play audio faster than what I could with pure native OpenSL ES from my brief test. It would be great if we could somehow supports this as Samsung phones are taking all the top 10 most popular phones in many countries.

### Nintendo Switch support

Depending on how successful I as a game developer can be after finishing the current game, the next game I want to make a Nintendo Switch exclusive game. And I will definitely take Native Audio with me. Let's see what Switch API offers and how much latency Unity adds to it.

But this is not a guarantee because my current game is the last try, if I can't make a living with it I will go to day job and likely cannot continue making that game on Switch. And I will likely not try to support other platforms that cannot be field-tested by my own game.

### No-copy audio loading utilizing [`NativeArray<T>`](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html)

Native Audio currently :

1. Do [`AudioClip.GetData`](https://docs.unity3d.com/ScriptReference/AudioClip.GetData.html) to an imported `AudioClip`. This requires an allocated `float[]` array.
2. Send that array's pointer to native side, native side then copy in C++ (`memcpy`) it to not rely on Unity side anymore. But you now have 2x cost of uncompressed audio at this moment.
3. The Unity side can now let the GC collect that `float[]` array to gain memory back to 1x uncompressed audio at native side, but collecting can also impact performance and garbage is bad in general.

The new workflow still does 1. (so technically that "get" is a "copy"), however it will be put in `NativeArray<float>` instead. Now the memory stays at C# always and also native side could use it via [`NativeArrayUnsafeUtility`](https://docs.unity3d.com/ScriptReference/Unity.Collections.LowLevel.Unsafe.NativeArrayUnsafeUtility.html). Here are some benefits :

- Unloading could be performed at C# side too (still dangerous as we need to make sure the native side is not currently reading it)
- Various crashes due to allocating memory would be more debuggable as the C# and Unity could tell us, not the native side hard crashing with SIGSEGV.
- We will not have to incur 2x uncompressed audio memory at any point, because we can allocate `NativeArray<float>` and make `GetData` see it as a target like `float[]`.
- Loading must be faster theoretically.
- "Less scary" as it enters managed realm. It feels more like the feature is supported/relied by Unity.

**This feature will move the lowest supported version to 2018.1**, as it was the first version to introduce [`NativeArray<T>`](https://docs.unity3d.com/ScriptReference/Unity.Collections.NativeArray_1.html).

(Reading/streaming memory out of `AudioClip` directly *without any copy* would be possible with DOTS Audio and its backend code introduced in 2019.1, but I think we need to see it ironed out more.)

But this feature has one difficult problem, the Secret Rabbit Code for resampling was easy to use if the memory is at native side being a native library. Now we must make it work on `NativeArray<float>` at C# instead. I don't know how many bugs and hardship I will face while doing that. (Resampled audio may need a differently sized destination, upsampling increases data for example.)