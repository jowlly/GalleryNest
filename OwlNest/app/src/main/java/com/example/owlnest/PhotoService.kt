package com.example.owlnest

import android.annotation.SuppressLint
import android.content.Context
import android.net.Uri
import android.provider.MediaStore
import androidx.media3.common.util.Log
import androidx.media3.common.util.UnstableApi
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.withContext
import okhttp3.MediaType.Companion.toMediaType
import okhttp3.MultipartBody
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.RequestBody.Companion.toRequestBody
import okhttp3.Response
import org.json.JSONArray
import org.json.JSONObject
import java.io.IOException
import java.text.SimpleDateFormat
import java.util.Date
import java.util.Locale
import java.util.concurrent.TimeUnit

@SuppressLint("StaticFieldLeak")
object PhotoService {
    var isInitialized: Boolean = false
    var servers: List<Server> = emptyList() // Список серверов
    var activeServer: Server? = null // Активный сервер
    var syncedIdPhotos: List<Int> = emptyList()

    private lateinit var context: Context

    suspend fun initialize(context: Context) {
        this.context = context
        loadServers()
        loadSynced()
        isInitialized = true
    }

    private suspend fun loadSynced() {
        val prefs = context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
        val syncedJson = prefs.getString("synced", null)
        syncedIdPhotos = if (syncedJson != null) {
            val jsonArray = JSONArray(syncedJson)
            List(jsonArray.length()) { index ->
                jsonArray.getInt(index)
            }
        } else {
            emptyList()
        }
    }

    private fun saveSynced() {
        val jsonArray = JSONArray().apply {
            syncedIdPhotos.forEach { put(it) }
        }
        context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
            .edit()
            .putString("synced", jsonArray.toString())
            .apply()
    }

    private suspend fun loadServers() {
        val prefs = context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
        val serversJson = prefs.getString("servers", null)
        servers = if (serversJson != null) {
            val jsonArray = JSONArray(serversJson)
            List(jsonArray.length()) { index ->
                val jsonObject = jsonArray.getJSONObject(index)
                Server(
                    id = jsonObject.getString("id"),
                    address = jsonObject.getString("address"),
                    isActive = jsonObject.optBoolean("isActive", false)
                )
            }
        } else {
            emptyList()
        }
        servers = checkServerAvailability(servers)
        activeServer = servers.find { it.isActive and it.isAvailable }
    }

    private fun saveServers() {
        val jsonArray = JSONArray()
        servers.forEach { server ->
            val jsonObject = JSONObject().apply {
                put("id", server.id)
                put("address", server.address)
                put("isActive", server.isActive)
            }
            jsonArray.put(jsonObject)
        }
        context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
            .edit()
            .putString("servers", jsonArray.toString())
            .apply()
    }

    fun addServer(server: Server) {
        servers = servers + server
        saveServers()
    }

    fun removeServer(server: Server) {
        servers = servers - server
        if (server.isActive) {
            activeServer = null
        }
        saveServers()
    }

    fun updateActiveServer(server: Server) {
        servers = servers.map {
            if (it.id == server.id) it.copy(isActive = true) else it.copy(isActive = false)
        }
        activeServer = server
        saveServers()
    }

    fun getPhotoUrl(photoId: Int): String {
        return "${activeServer?.address}/api/photo/download?photoId=$photoId"
    }

    suspend fun checkServerAvailability(serverList: List<Server> = emptyList()): List<Server> {
        return coroutineScope {
            serverList.map { server ->
                async {
                    try {
                        val client = OkHttpClient.Builder()
                            .connectTimeout(5, TimeUnit.SECONDS)
                            .build()

                        val request = Request.Builder()
                            .url("${server.address}/api/device/connect")
                            .build()

                        val response = withContext(Dispatchers.IO) {
                            client.newCall(request).execute()
                        }
                        server.copy(isAvailable = response.isSuccessful)
                    } catch (e: Exception) {
                        server.copy(isAvailable = false)
                    }
                }
            }.awaitAll()
        }
    }

    suspend fun fetchPhotos(): List<Photo> {
        val client = OkHttpClient()
        val request = Request.Builder()
            .url("${activeServer?.address}/api/photo/meta")
            .build()

        return withContext(Dispatchers.IO) {
            val response: Response = client.newCall(request).execute()
            val jsonArray = JSONArray(response.body?.string())
            List(jsonArray.length()) { index ->
                val jsonObject = jsonArray.getJSONObject(index)
                val albumIds = mutableListOf<Int>()
                when (val albumIdValue = jsonObject["albumIds"]) {
                    is JSONArray -> {
                        for (i in 0 until albumIdValue.length()) {
                            albumIds.add(albumIdValue.getInt(i))
                        }
                    }

                    is Int -> {
                        albumIds.add(albumIdValue)
                    }

                    else -> {
                        try {
                            albumIds.add(albumIdValue.toString().toInt())
                        } catch (e: NumberFormatException) {
                        }
                    }
                }

                Photo(
                    id = jsonObject.getInt("id"),
                    albumId = albumIds,
                    path = jsonObject.getString("path")
                )
            }
        }
    }

    @androidx.annotation.OptIn(UnstableApi::class)
    public suspend fun uploadImages(context: Context, uris: List<Uri>) {
        val client = OkHttpClient()

        uris.forEach { uri ->
            try {
                context.contentResolver.openInputStream(uri)?.use { inputStream ->
                    val bytes = withContext(Dispatchers.IO) {
                        inputStream.readBytes()
                    }

                    // Получаем дату создания фото из метаданных
                    val dateTaken = getCreationDate(context, uri)
                    val formattedDate = dateTaken?.let {
                        SimpleDateFormat("yyyy-MM-dd'T'HH:mm:ss", Locale.ENGLISH)
                            .format(Date(it))
                    }
                    val requestBody = MultipartBody.Builder()
                        .setType(MultipartBody.FORM)
                        .addFormDataPart(
                            "file",
                            "photo_${System.currentTimeMillis()}.jpg",
                            bytes.toRequestBody("image/jpeg".toMediaType())
                        )
                        .apply {
                            formattedDate?.let {
                                addFormDataPart("creationTime", it)
                            }
                        }
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

    private fun getCreationDate(context: Context, uri: Uri): Long? {
        val projection = arrayOf(
            MediaStore.Images.ImageColumns.DATE_TAKEN
        )

        return context.contentResolver.query(
            uri,
            projection,
            null,
            null,
            null
        )?.use { cursor ->
            if (cursor.moveToFirst()) {
                val dateIndex =
                    cursor.getColumnIndexOrThrow(MediaStore.Images.ImageColumns.DATE_TAKEN)
                if (!cursor.isNull(dateIndex)) {
                    cursor.getLong(dateIndex)
                } else {
                    null
                }
            } else {
                null
            }
        }
    }
}
data class Photo(val id: Int, val albumId: List<Int>, val path: String)

data class Server(
    val id: String,
    val address: String,
    val isActive: Boolean = false,
    val isAvailable: Boolean = false
)