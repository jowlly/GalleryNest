package com.example.owlnest

import android.annotation.SuppressLint
import android.content.Context
import android.provider.MediaStore
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.async
import kotlinx.coroutines.awaitAll
import kotlinx.coroutines.coroutineScope
import kotlinx.coroutines.withContext
import okhttp3.OkHttpClient
import okhttp3.Request
import okhttp3.Response
import org.json.JSONArray
import org.json.JSONObject
import java.util.concurrent.TimeUnit

@SuppressLint("StaticFieldLeak")
object PhotoService {
    var isInitialized: Boolean = false
    var servers: List<Server> = emptyList() // Список серверов
    var activeServer: Server? = null // Активный сервер

    private lateinit var context: Context

    suspend fun initialize(context: Context) {
        this.context = context
        loadServers()
        isInitialized = true
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

    suspend fun checkServerAvailability(serverList: List<Server> = emptyList()) : List<Server> {
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
                Photo(
                    id = jsonObject.getInt("id"),
                    albumId = jsonObject.getInt("albumId"),
                    path = jsonObject.getString("path")
                )
            }
        }
    }

}

data class Photo(val id: Int, val albumId: Int, val path: String)

data class Server(
    val id: String,
    val address: String,
    val isActive: Boolean = false,
    val isAvailable: Boolean = false
)