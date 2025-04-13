package com.example.owlnest

import android.Manifest
import android.content.Context
import android.util.Size
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalLifecycleOwner
import androidx.core.content.ContextCompat
import androidx.navigation.NavController
import kotlinx.coroutines.launch
import okhttp3.internal.wait
import java.util.concurrent.Executors

@Composable
fun QrScannerScreen(navController: NavController) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current
    val cameraExecutor = remember { Executors.newSingleThreadExecutor() }
    val scope = rememberCoroutineScope()

    val permissionLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        if (isGranted) {
            // Permission granted, start camera
        }
    }

    LaunchedEffect(Unit) {
        permissionLauncher.launch(Manifest.permission.CAMERA)
    }

    Box(modifier = Modifier.fillMaxSize()) {
        CameraPreview(
            context = context,
            lifecycleOwner = lifecycleOwner,
            executor = cameraExecutor,
            onBarcodeDetected = { barcodeValue ->
                scope.launch {
                    handleScannedBarcode(barcodeValue, context, navController)
                }
            }
        )

        Text(
            text = "Наведите камеру на QR-код",
            modifier = Modifier.align(Alignment.TopCenter)
        )
    }
}

private suspend fun handleScannedBarcode(
    barcodeValue: String,
    context: Context,
    navController: NavController
) {
    if (barcodeValue.startsWith("http")) {

        val prefix = "http://"
        var urls = barcodeValue.removePrefix(prefix)
            .substringBeforeLast(':')
            .split('|')
            .map { "$prefix$it:${barcodeValue.substringAfterLast(':')}" }
            .map{
                Server(
                id = System.currentTimeMillis().toString(),
                    address = it,
                    isActive = false,
                isAvailable = false,
            )}

        urls = PhotoService.checkServerAvailability(urls)
        urls.filter { it.isAvailable }.forEach {
            PhotoService.addServer(it)
        }

        navController.navigate("source") {
            popUpTo("scanner") { inclusive = true }
        }
    }
}