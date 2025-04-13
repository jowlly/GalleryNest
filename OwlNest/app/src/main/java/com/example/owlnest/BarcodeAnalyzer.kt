package com.example.owlnest

import android.graphics.ImageFormat
import androidx.annotation.OptIn
import androidx.camera.core.ExperimentalGetImage
import androidx.camera.core.ImageAnalysis
import androidx.camera.core.ImageProxy
import com.google.mlkit.vision.barcode.BarcodeScanner
import com.google.mlkit.vision.barcode.BarcodeScanning
import com.google.mlkit.vision.barcode.common.Barcode
import com.google.mlkit.vision.common.InputImage
import java.util.concurrent.Executors

class BarcodeAnalyzer(
    private val onBarcodeDetected: (barcode: Barcode?) -> Unit
) : ImageAnalysis.Analyzer {
    private val scanner: BarcodeScanner = BarcodeScanning.getClient()
    private val executor = Executors.newSingleThreadExecutor()

    @OptIn(ExperimentalGetImage::class)
    override fun analyze(imageProxy: ImageProxy) {
        val mediaImage = imageProxy.image
        if (mediaImage != null && mediaImage.format == ImageFormat.YUV_420_888) {
            val image = InputImage.fromMediaImage(mediaImage, imageProxy.imageInfo.rotationDegrees)

            scanner.process(image)
                .addOnSuccessListener { barcodes ->
                    barcodes.firstOrNull()?.let {
                        onBarcodeDetected(it)
                    }
                }
                .addOnCompleteListener {
                    imageProxy.close()
                }
        }
    }
}