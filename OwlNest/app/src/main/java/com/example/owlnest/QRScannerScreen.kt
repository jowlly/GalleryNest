package com.example.owlnest

import android.Manifest
import android.content.Context
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.Text
import androidx.compose.runtime.*
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.platform.LocalLifecycleOwner
import androidx.navigation.NavController
import kotlinx.coroutines.launch
import java.util.concurrent.Executors

@Composable
fun QrScannerScreen(navController: NavController) {
    val context = LocalContext.current
    val lifecycleOwner = LocalLifecycleOwner.current
    val cameraExecutor = remember { Executors.newSingleThreadExecutor() }
    val scope = rememberCoroutineScope()
    var isProcessing by remember { mutableStateOf(false) } // Флаг обработки

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
                if (!isProcessing) {
                    isProcessing = true
                    scope.launch {
                        handleScannedBarcode(barcodeValue, context, navController)
                    }
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
            .map {
                Server(
                    id = System.currentTimeMillis().toString(),
                    address = it,
                    isActive = false,
                    isAvailable = false,
                )
            }

        urls = PhotoService.checkServerAvailability(urls)
        urls.filter { it.isAvailable }.forEach {
            PhotoService.addServer(it)
        }

        navController.navigate("source") {
            popUpTo("scanner") { inclusive = true }
        }
    }
}