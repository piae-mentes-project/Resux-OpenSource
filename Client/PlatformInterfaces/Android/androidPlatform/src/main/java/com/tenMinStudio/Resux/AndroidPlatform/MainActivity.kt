package com.tenMinStudio.Resux.AndroidPlatform

import android.Manifest
import android.bluetooth.BluetoothAdapter
import android.bluetooth.BluetoothDevice
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.content.pm.PackageManager
import android.os.Build
import com.unity3d.player.UnityPlayerActivity
import android.os.Bundle
import android.util.Log
import android.widget.Toast

class MainActivity : UnityPlayerActivity() {
    private var isInitialized: Boolean = false
    private lateinit var bluetoothHeadsetReceiver: BluetoothHeadsetReceiver;

    companion object {
        @JvmStatic
        var instance: MainActivity? = null
    }

    override fun onCreate(savedInstanceState: Bundle?) {
        Log.i("Resux", "Resux start!")
        super.onCreate(savedInstanceState)

        instance = this

        if (!isInitialized) {
            isInitialized = true
            initUnityInterface()
        }
        Log.i("Resux", "Resux inited.")
    }

    private val bluetoothEventListener: BroadcastReceiver =
        object : BroadcastReceiver() {
            override fun onReceive(context: Context?, intent: Intent?) {
                Log.i("Resux", "broadcast received(main thread).")
            }
        }

    private val bluetoothRequestCode = 114514
    override fun onRequestPermissionsResult(
        requestCode: Int,
        permissions: Array<out String>,
        grantResults: IntArray
    ) {
        super.onRequestPermissionsResult(requestCode, permissions, grantResults)
        when (requestCode) {
            bluetoothRequestCode -> {
                if (grantResults.any() && grantResults.all { it == PackageManager.PERMISSION_GRANTED }) {
                    bluetoothHeadsetReceiver.register(this)
                } else {
                    Toast.makeText(this, "我们需要蓝牙权限，这样当蓝牙耳机连接时我们可以自动进行优化。\nWe need Bluetooth access so that we can automatically optimize when the Bluetooth headset is connected", Toast.LENGTH_LONG).show()
                }
            }
        }
    }

    private fun initUnityInterface() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.S) {
            requestPermissions(
                arrayOf(
                    Manifest.permission.BLUETOOTH,
                    Manifest.permission.BLUETOOTH_CONNECT
                ), bluetoothRequestCode
            )
        }

        bluetoothHeadsetReceiver = BluetoothHeadsetReceiver()
    }

    fun getBluetoothHeadsetReceiver(): BluetoothHeadsetReceiver {
        return bluetoothHeadsetReceiver
    }
}