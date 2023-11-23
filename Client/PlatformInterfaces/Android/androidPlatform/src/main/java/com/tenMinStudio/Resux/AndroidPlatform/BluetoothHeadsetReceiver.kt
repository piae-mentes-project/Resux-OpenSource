package com.tenMinStudio.Resux.AndroidPlatform

import android.Manifest
import android.annotation.SuppressLint
import android.app.Activity
import android.bluetooth.*
import android.content.BroadcastReceiver
import android.content.Context
import android.content.Intent
import android.content.IntentFilter
import android.content.pm.PackageManager
import android.media.AudioManager
import android.os.Build
import android.util.Log
import kotlin.concurrent.thread

class BluetoothHeadsetReceiver {
    private val listeners: ArrayList<BluetoothHeadsetCallback> = ArrayList()

    private var lastState = false

    @SuppressLint("MissingPermission")
    private fun checkBluetoothHeadset() {
        if (bluetoothAdapter == null) return
        val state =
            (BluetoothProfile.STATE_CONNECTED == bluetoothAdapter!!.getProfileConnectionState(
                BluetoothProfile.HEADSET
            ))
        if (lastState == state) return
        lastState = state
        emit(state)
    }

    private fun emit(value: Boolean) {
        Log.v("Resux", "BluetoothHeadsetReceiver emit with $value")
        listeners.forEach { it.stateChanged(value) }
    }

    private var bluetoothAdapter: BluetoothAdapter? = null
    public fun register(context: Activity): Boolean {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M &&
            context.checkSelfPermission(Manifest.permission.BLUETOOTH_CONNECT) != PackageManager.PERMISSION_GRANTED
        ) {
            return false
        }

        val bluetoothManager =
            context.getSystemService(Context.BLUETOOTH_SERVICE) as BluetoothManager
        bluetoothAdapter = bluetoothManager.adapter

        thread {
            while (true) {
                checkBluetoothHeadset()
                Thread.sleep(1919)
            }
        }
        return true
    }

    public fun addListener(listener: BluetoothHeadsetCallback) {
        listeners.add(listener)
        listener.stateChanged(lastState)
    }

    public fun removeListener(listener: BluetoothHeadsetCallback) {
        listeners.remove(listener)
    }

    public fun removeAllListener() {
        listeners.clear()
    }
}