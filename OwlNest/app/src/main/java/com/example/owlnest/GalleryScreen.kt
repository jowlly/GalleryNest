package com.example.owlnest

import android.Manifest
import android.R.color
import android.content.Context
import android.net.Uri
import android.os.Build
import android.provider.MediaStore
import androidx.compose.animation.animateContentSize
import androidx.compose.foundation.ExperimentalFoundationApi
import androidx.compose.foundation.background
import androidx.compose.foundation.border
import androidx.compose.foundation.combinedClickable
import androidx.compose.foundation.gestures.awaitEachGesture
import androidx.compose.foundation.gestures.awaitFirstDown
import androidx.compose.foundation.gestures.calculateCentroid
import androidx.compose.foundation.gestures.calculateCentroidSize
import androidx.compose.foundation.gestures.calculateZoom
import androidx.compose.foundation.gestures.detectTransformGestures
import androidx.compose.foundation.gestures.rememberTransformableState
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.BoxWithConstraints
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.foundation.layout.fillMaxWidth
import androidx.compose.foundation.layout.padding
import androidx.compose.foundation.layout.size
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.automirrored.filled.Send
import androidx.compose.material.icons.filled.Send
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.material3.FloatingActionButton
import androidx.compose.material3.Icon
import androidx.compose.material3.Text
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.runtime.getValue
import androidx.compose.runtime.mutableStateOf
import androidx.compose.runtime.remember
import androidx.compose.runtime.setValue
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.draw.drawWithCache
import androidx.compose.ui.geometry.Offset
import androidx.compose.ui.graphics.BlendMode
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.input.pointer.PointerEventPass
import androidx.compose.ui.input.pointer.PointerInputChange
import androidx.compose.ui.input.pointer.PointerInputScope
import androidx.compose.ui.input.pointer.pointerInput
import androidx.compose.ui.layout.ContentScale
import androidx.compose.ui.platform.LocalConfiguration
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.tooling.preview.Preview
import androidx.compose.ui.unit.dp
import androidx.lifecycle.ViewModel
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.navigation.NavController
import coil.ImageLoader
import coil.compose.AsyncImage
import coil.request.ImageRequest
import coil.size.Size
import coil.request.CachePolicy
import com.google.accompanist.permissions.ExperimentalPermissionsApi
import com.google.accompanist.permissions.isGranted
import com.google.accompanist.permissions.rememberPermissionState
import com.google.accompanist.permissions.shouldShowRationale
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.withContext
import kotlin.math.abs

class GalleryViewModel : ViewModel() {
    var images by mutableStateOf(emptyList<Uri>())
        private set

    var isLoading by mutableStateOf(true)
        private set

    var selectedImages by mutableStateOf(emptySet<Uri>())
        private set

    fun toggleImageSelection(uri: Uri) {
        selectedImages = if (selectedImages.contains(uri)) {
            selectedImages - uri
        } else {
            selectedImages + uri
        }
    }

    suspend fun loadImages(context: Context) {
        withContext(Dispatchers.IO) {
            val result = mutableListOf<Uri>()
            val projection = arrayOf(MediaStore.Images.Media._ID)

            context.contentResolver.query(
                MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
                projection,
                null,
                null,
                "${MediaStore.Images.Media.DATE_ADDED} DESC"
            )?.use { cursor ->
                val idColumn = cursor.getColumnIndexOrThrow(MediaStore.Images.Media._ID)
                while (cursor.moveToNext()) {
                    val id = cursor.getLong(idColumn)
                    val contentUri = Uri.withAppendedPath(
                        MediaStore.Images.Media.EXTERNAL_CONTENT_URI,
                        id.toString()
                    )
                    result.add(contentUri)
                }
            }
            images = result
            isLoading = false
        }
    }

}

@OptIn(ExperimentalPermissionsApi::class)
@Composable
fun GalleryScreen(navController:NavController, viewModel: GalleryViewModel = viewModel()) {
    val context = LocalContext.current
    val permission = if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.TIRAMISU) {
        Manifest.permission.READ_MEDIA_IMAGES
    } else {
        Manifest.permission.READ_EXTERNAL_STORAGE
    }

    val permissionState = rememberPermissionState(permission)

    LaunchedEffect(permissionState.status) {
        if (permissionState.status.isGranted) {
            viewModel.loadImages(context)
        }
    }

    Box(modifier = Modifier.fillMaxSize()) {
        when {
            permissionState.status.isGranted -> {
                if (viewModel.isLoading) {
                    CircularProgressIndicator(modifier = Modifier.align(Alignment.Center))
                } else {
                    ImageGrid(images = viewModel.images,
                        selectedImages = viewModel.selectedImages,
                        onImageSelected = viewModel::toggleImageSelection,
                        navController = navController)
                }
            }

            permissionState.status.shouldShowRationale -> {
                Text("Permission is required to access gallery", modifier = Modifier.align(Alignment.Center))
            }

            else -> {
                LaunchedEffect(Unit) {
                    permissionState.launchPermissionRequest()
                }
                Text("Requesting permission...", modifier = Modifier.align(Alignment.Center))
            }
        }
        if (viewModel.selectedImages.isNotEmpty()) {
            FloatingActionButton(
                onClick = { navController.navigate("source") },
                modifier = Modifier
                    .align(Alignment.BottomEnd)
                    .padding(16.dp),
                containerColor = Color.Blue,
                contentColor = Color.White
            ) {
                Icon(Icons.AutoMirrored.Filled.Send, contentDescription = "Send")
            }
        }
    }
}

@Suppress("LongMethod", "ComplexMethod")
suspend fun PointerInputScope.detectPinchGestures(
    pass: PointerEventPass = PointerEventPass.Main,
    onGestureStart: (PointerInputChange) -> Unit = {},
    onGesture: (
        centroid: Offset,
        zoom: Float
    ) -> Unit,
    onGestureEnd: (PointerInputChange) -> Unit = {}
) {
    awaitEachGesture {
        var zoom = 1f
        var pastTouchSlop = false
        val touchSlop = viewConfiguration.touchSlop
        val down: PointerInputChange = awaitFirstDown(requireUnconsumed = false, pass = pass)
        onGestureStart(down)
        var pointer = down
        var pointerId = down.id
        do {
            val event = awaitPointerEvent(pass = pass)
            val canceled = event.changes.any { it.isConsumed }
            if (!canceled) {
                val pointerInputChange = event.changes.firstOrNull { it.id == pointerId } ?: event.changes.first()
                pointerId = pointerInputChange.id
                pointer = pointerInputChange
                val zoomChange = event.calculateZoom()
                if (!pastTouchSlop) {
                    zoom *= zoomChange
                    val centroidSize = event.calculateCentroidSize(useCurrent = false)
                    val zoomMotion = abs(1 - zoom) * centroidSize
                    if (zoomMotion > touchSlop) {
                        pastTouchSlop = true
                    }
                }
                if (pastTouchSlop) {
                    val centroid = event.calculateCentroid(useCurrent = false)
                    if (zoomChange != 1f) {
                        onGesture(
                            centroid,
                            zoomChange
                        )
                        event.changes.forEach { it.consume() }
                    }
                }
            }
        } while (!canceled && event.changes.any { it.pressed })
        onGestureEnd(pointer)
    }
}

@OptIn(ExperimentalFoundationApi::class)
@Composable
private fun ImageGrid(
    images: List<Uri>,
    selectedImages: Set<Uri>,
    onImageSelected: (Uri) -> Unit,
    navController: NavController)
{
    val context = LocalContext.current
    var zoom by remember{ mutableStateOf(1f) }
    var columnCount by remember { mutableStateOf(3) }
    val configuration = LocalConfiguration.current
    val screenWidth = configuration.screenWidthDp.dp
    val maxColumns = 4
    val minColumns = 1
    val itemSize = (screenWidth / columnCount) - 2.dp

    LazyVerticalGrid(
        columns = GridCells.Fixed(columnCount),
        modifier = Modifier
            .fillMaxSize()
            .pointerInput(Unit){
                detectPinchGestures(
                    pass = PointerEventPass.Initial,
                    onGesture = { centroid: Offset, newZoom: Float ->
                        val newScale = (zoom * newZoom)
                        if (newScale > 1.25f) {
                            columnCount = columnCount.dec().coerceIn(minColumns, maxColumns)
                            zoom = 1f
                        } else if (newScale < 0.75f) {
                            columnCount = columnCount.inc().coerceIn(minColumns, maxColumns)
                            zoom = 1f
                        } else {
                            zoom = newScale
                        }
                    },
                    onGestureEnd = { zoom = 1f }
                )
            }
    ) {
        items(images.size) { index ->
            val uri = images[index]
            val isSelected = selectedImages.contains(uri)
            Box(
                modifier = Modifier
                    .padding(1.dp)
                    .combinedClickable(
                        onClick = { /* Обработка обычного клика при необходимости */ },
                        onLongClick = { onImageSelected(uri) }
                    )
                    .drawWithCache {
                        val gradient = if (isSelected) {
                            Brush.verticalGradient(
                                colors = listOf(Color.Transparent, Color.Black.copy(alpha = 0.4f)),
                                startY = 0f,
                                endY = size.height
                            )
                        } else Brush.linearGradient(emptyList())

                        onDrawWithContent {
                            drawContent()
                            if (isSelected) {
                                drawRect(
                                    brush = gradient,
                                    blendMode = BlendMode.Darken
                                )
                            }
                        }
                    }
            ) {
            AsyncImage(
                model = ImageRequest.Builder(context)
                    .data(uri)
                    .crossfade(true)
                    .size(itemSize.value.toInt(),itemSize.value.toInt())
                    .diskCachePolicy(CachePolicy.ENABLED)
                    .memoryCachePolicy(CachePolicy.ENABLED)
                    .diskCacheKey(images[index].toString())
                    .memoryCacheKey(images[index].toString())
                    .build(),
                contentDescription = "Gallery image",
                contentScale = ContentScale.Crop,
                modifier = Modifier
                    .size(itemSize)
                    .padding(1.dp)
                    .animateItem()
                    .border(
                        width = if (isSelected) 3.dp else 0.dp,
                        color = Color.Blue,
                        shape = RoundedCornerShape(4.dp)
                    )
            )
                if (isSelected) {
                    Text(
                        text = "${selectedImages.indexOf(uri) + 1}",
                        color = Color.White,
                        modifier = Modifier
                            .align(Alignment.TopEnd)
                            .padding(4.dp)
                            .background(Color.Blue, CircleShape)
                            .padding(4.dp)
                    )
                }
            }
        }
    }
}
