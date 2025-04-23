package com.example.owlnest

import android.Manifest
import android.content.ContentValues
import android.content.Context
import android.content.pm.PackageManager
import android.media.MediaScannerConnection
import android.net.Uri
import android.os.Build
import android.os.Environment
import android.provider.MediaStore
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.ActivityResultLauncher
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.ExperimentalFoundationApi
import androidx.compose.foundation.background
import androidx.compose.foundation.combinedClickable
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.Column
import androidx.compose.foundation.layout.PaddingValues
import androidx.compose.foundation.layout.Spacer
import androidx.compose.foundation.layout.aspectRatio
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.height
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.itemsIndexed
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.AddCircle
import androidx.compose.material.icons.filled.CheckCircle
import androidx.compose.material.icons.filled.Close
import androidx.compose.material3.Button
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.ExperimentalMaterial3Api
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.IconButton
import androidx.compose.material3.Scaffold
import androidx.compose.material3.Text
import androidx.compose.material3.TopAppBar
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateListOf
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.rememberCoroutineScope
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.unit.dp
import androidx.core.content.ContextCompat
import androidx.media3.common.util.Log
import androidx.media3.common.util.UnstableApi
import androidx.navigation.NavController
import coil3.compose.AsyncImage
import coil3.request.CachePolicy
import coil3.request.ImageRequest
import coil3.size.Size
import kotlinx.coroutines.CoroutineScope
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.MultipartBody
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import java.io.IOException

@Composable
fun ServerGalleryScreen(navController: NavController) {
    val photos = remember { mutableStateListOf<Photo>() }
    val selectedPhotos = remember { mutableStateListOf<Int>() }
    val context = LocalContext.current
    var showError by remember { mutableStateOf(false) }
    var isLoading by remember { mutableStateOf(false) }
    var isUploading by remember { mutableStateOf(false) }
    var isDownloading by remember { mutableStateOf(false) }
    val lifecycleScope = rememberCoroutineScope()

    val imagePicker = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.GetMultipleContents(),
        onResult = { uris ->
            if (uris.isNotEmpty()) {
                isUploading = true
                lifecycleScope.launch {
                    uploadImages(context, uris)
                    photos.clear()
                    photos.addAll(PhotoService.fetchPhotos())
                    isUploading = false
                }
            }
        }
    )

    val permissionLauncher = rememberLauncherForActivityResult(
        ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        if (isGranted) {
            lifecycleScope.launch {
                isDownloading = true
                downloadSelectedPhotos(context, selectedPhotos.toList())
                selectedPhotos.clear()
                isDownloading = false
            }
        }
    }

    LaunchedEffect(Unit) {
        if (PhotoService.activeServer == null) {
            showError = true
            return@LaunchedEffect
        }

        try {
            isLoading = true
            val fetchedPhotos = PhotoService.fetchPhotos()
            photos.clear()
            photos.addAll(fetchedPhotos)
            isLoading = false
        } catch (e: Exception) {
            isLoading = false
            showError = true
        }
    }

    Scaffold(
        topBar = {
            if (selectedPhotos.isNotEmpty()) {
                SelectionTopAppBar(
                    selectedCount = selectedPhotos.size,
                    onDownloadClick = {
                        handleDownload(
                            context, selectedPhotos.toList(), permissionLauncher,
                            {
                                isDownloading = it
                            },lifecycleScope
                        )
                    },
                    onCloseClick = { selectedPhotos.clear() }
                )
            }
        },
        floatingActionButton = {
            if (selectedPhotos.isEmpty()) {
                FloatingActionButton(
                    onClick = { imagePicker.launch("image/*") },
                    modifier = Modifier.padding(16.dp)
                ) {
                    Icon(Icons.Default.Add, contentDescription = "Upload photos")
                }
            }
        }
    ) { paddingValues ->
        Box(
            modifier = Modifier
                .fillMaxSize()
                .padding(paddingValues)
        ) {
            when {
                isLoading -> LoadingIndicator()
                showError -> ErrorScreen(
                    message = "Ошибка подключения к серверу",
                    onRetry = {
                        showError = false
                        // Можно добавить повторную загрузку
                    }
                )
                else -> PhotoGrid(
                    photos = photos,
                    selectedPhotos = selectedPhotos,
                    onPhotoSelected = { photoId ->
                        if (selectedPhotos.contains(photoId)) {
                            selectedPhotos.remove(photoId)
                        } else {
                            selectedPhotos.add(photoId)
                        }
                    }
                )
            }

            if (isUploading) {
                UploadProgressIndicator(message = "Загрузка фотографий...")
            }

            if (isDownloading) {
                UploadProgressIndicator(message = "Скачивание фотографий...")
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun SelectionTopAppBar(
    selectedCount: Int,
    onDownloadClick: () -> Unit,
    onCloseClick: () -> Unit
) {
    TopAppBar(
        title = { Text("Выбрано: $selectedCount") },
        actions = {
            IconButton(onClick = onDownloadClick) {
                Icon(Icons.Default.AddCircle, contentDescription = "Download")
            }
            IconButton(onClick = onCloseClick) {
                Icon(Icons.Default.Close, contentDescription = "Clear selection")
            }
        }
    )
}

@OptIn(ExperimentalFoundationApi::class)
@Composable
private fun PhotoGrid(
    photos: List<Photo>,
    selectedPhotos: List<Int>,
    onPhotoSelected: (Int) -> Unit
) {
    LazyVerticalGrid(
        columns = GridCells.Fixed(2),
        contentPadding = PaddingValues(4.dp)
    ) {
        itemsIndexed(photos) { _, photo ->
            PhotoItem(
                photo = photo,
                isSelected = selectedPhotos.contains(photo.id),
                onLongClick = { onPhotoSelected(photo.id) }
            )
        }
    }
}

@OptIn(ExperimentalFoundationApi::class)
@Composable
private fun PhotoItem(
    photo: Photo,
    isSelected: Boolean,
    onLongClick: () -> Unit
) {
    Box(
        modifier = Modifier
            .padding(4.dp)
            .aspectRatio(1f)
            .clip(RoundedCornerShape(8.dp))
            .combinedClickable(
                onLongClick = onLongClick,
                onClick = {}
            )
    ) {
        AsyncImage(
            model = ImageRequest.Builder(LocalContext.current)
                .data(PhotoService.getPhotoUrl(photo.id))
                .size(Size.ORIGINAL)
                .memoryCachePolicy(CachePolicy.DISABLED)
                .diskCachePolicy(CachePolicy.DISABLED)
                .build(),
            contentDescription = null,
            modifier = Modifier.fillMaxSize()
        )

        if (isSelected) {
            Box(
                modifier = Modifier
                    .fillMaxSize()
                    .background(Color.Black.copy(alpha = 0.4f)),
                contentAlignment = Alignment.Center
            ) {
                Icon(
                    imageVector = Icons.Default.CheckCircle,
                    contentDescription = "Selected",
                    tint = Color.White,
                    modifier = Modifier.size(48.dp)
                )
            }
        }
    }
}

@Composable
private fun LoadingIndicator() {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        CircularProgressIndicator()
    }
}

@Composable
private fun UploadProgressIndicator(message: String) {
    Box(
        modifier = Modifier
            .fillMaxSize()
            .background(Color.Black.copy(alpha = 0.7f)),
        contentAlignment = Alignment.Center
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            CircularProgressIndicator(color = Color.White)
            Spacer(modifier = Modifier.height(8.dp))
            Text(
                text = message,
                color = Color.White
            )
        }
    }
}

@Composable
fun ErrorScreen(message: String, onRetry: () -> Unit) {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        Column(horizontalAlignment = Alignment.CenterHorizontally) {
            Text(text = message)
            Spacer(modifier = Modifier.height(16.dp))
            Button(onClick = onRetry) {
                Text("Повторить попытку")
            }
        }
    }
}

private fun handleDownload(
    context: Context,
    selectedPhotos: List<Int>,
    permissionLauncher: ActivityResultLauncher<String>,
    onDownloadStateChange: (Boolean) -> Unit,
    lifecycleScope: CoroutineScope
) {
    if (Build.VERSION.SDK_INT < Build.VERSION_CODES.Q &&
        ContextCompat.checkSelfPermission(
            context,
            Manifest.permission.WRITE_EXTERNAL_STORAGE
        ) != PackageManager.PERMISSION_GRANTED
    ) {
        permissionLauncher.launch(Manifest.permission.WRITE_EXTERNAL_STORAGE)
    } else {
        lifecycleScope.launch {
            onDownloadStateChange(true)
            downloadSelectedPhotos(context, selectedPhotos)
            onDownloadStateChange(false)
        }
    }
}

@androidx.annotation.OptIn(UnstableApi::class)
private suspend fun downloadSelectedPhotos(context: Context, photoIds: List<Int>) {
    val client = OkHttpClient()

    photoIds.forEach { id ->
        try {
            val url = PhotoService.getPhotoUrl(id)
            val request = Request.Builder().url(url).build()

            val response = withContext(Dispatchers.IO) {
                client.newCall(request).execute()
            }

            if (response.isSuccessful) {
                response.body?.bytes()?.let { bytes ->
                    saveImageToGallery(context, bytes, "photo_$id.jpg")
                }
            }
        } catch (e: IOException) {
            Log.e("ServerGalleryScreen", "Error downloading photo $id", e)
        }
    }
}

private fun saveImageToGallery(context: Context, bytes: ByteArray, fileName: String) {
    val contentValues = ContentValues().apply {
        put(MediaStore.Images.Media.DISPLAY_NAME, fileName)
        put(MediaStore.Images.Media.MIME_TYPE, "image/jpeg")
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
            put(MediaStore.Images.Media.RELATIVE_PATH, "${Environment.DIRECTORY_PICTURES}/Owlnest")
            put(MediaStore.Images.Media.IS_PENDING, 1)
        }
    }

    val uri = context.contentResolver.insert(
        MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
        contentValues
    ) ?: return

    context.contentResolver.openOutputStream(uri)?.use { outputStream ->
        outputStream.write(bytes)
    }

    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q) {
        contentValues.clear()
        contentValues.put(MediaStore.Images.Media.IS_PENDING, 0)
        context.contentResolver.update(uri, contentValues, null, null)
    }

    MediaScannerConnection.scanFile(
        context,
        arrayOf(uri.toString()),
        arrayOf("image/jpeg"),
        null
    )
}

@androidx.annotation.OptIn(UnstableApi::class)
private suspend fun uploadImages(context: Context, uris: List<Uri>) {
    val client = OkHttpClient()

    uris.forEach { uri ->
        try {
            context.contentResolver.openInputStream(uri)?.use { inputStream ->
                val bytes = withContext(Dispatchers.IO) {
                    inputStream.readBytes()
                }

                val requestBody = MultipartBody.Builder()
                    .setType(MultipartBody.FORM)
                    .addFormDataPart(
                        "file",
                        "photo_${System.currentTimeMillis()}.jpg",
                        bytes.toRequestBody("image/jpeg".toMediaType())
                    )
                    .build()

                val request = Request.Builder()
                    .url("${PhotoService.activeServer?.address}/api/photo/upload")
                    .post(requestBody)
                    .build()

                val response = client.newCall(request).execute()
                if (!response.isSuccessful) {
                    throw IOException("Ошибка загрузки: ${response.code}")
                }
            }
        } catch (e: Exception) {
            Log.e("ServerGalleryScreen", "Error uploading image", e)
        }
    }
}