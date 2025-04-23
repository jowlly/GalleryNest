package com.example.owlnest

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Add
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.Share
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.unit.dp
import androidx.navigation.NavController

@Composable
fun SourceScreen(navController: NavController) {
    var servers by remember { mutableStateOf(PhotoService.servers) }
    var showAddServerDialog by remember { mutableStateOf(false) }
    var serverAddress by remember { mutableStateOf("") }

    fun onAddServer(address: String) {
        val newServer = Server(
            id = System.currentTimeMillis().toString(),
            address = address
        )
        PhotoService.addServer(newServer)
        servers = PhotoService.servers
    }

    fun onRemoveServer(server: Server) {
        PhotoService.removeServer(server)
        servers = PhotoService.servers
    }

    fun onSetActiveServer(server: Server) {
        PhotoService.updateActiveServer(server)
        servers = PhotoService.servers
    }

    if (showAddServerDialog) {
        AlertDialog(
            onDismissRequest = { showAddServerDialog = false },
            title = { Text("Добавить сервер") },
            text = {
                Column {
                    OutlinedTextField(
                        value = serverAddress,
                        onValueChange = { serverAddress = it },
                        label = { Text("Введите IP-адрес сервера") },
                        modifier = Modifier.fillMaxWidth()
                    )
                    Spacer(modifier = Modifier.height(16.dp))
                    Button(
                        onClick = {
                            navController.navigate("scanner")
                            showAddServerDialog = false
                        },
                        modifier = Modifier.fillMaxWidth()
                    ) {
                        Text("Сканировать QR-код")
                    }
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        if (serverAddress.isNotBlank()) {
                            onAddServer(serverAddress) // Добавляем сервер
                        }
                    }
                ) {
                    Text("Добавить")
                }
            },
            dismissButton = {
                Button(
                    onClick = { showAddServerDialog = false } // Закрываем диалог
                ) {
                    Text("Отмена")
                }
            }
        )
    }


    Scaffold(
        floatingActionButton = {
            Column(){
                FloatingActionButton(onClick = {  showAddServerDialog = true}) {
                    Icon(Icons.Default.Add, contentDescription = "Add server")
                }
                FloatingActionButton(onClick = { navController.navigate("server")}) {
                    Icon(Icons.Default.Share, contentDescription = "Show server gallery")
                }
            }
        }
    ) { padding ->
        LazyColumn(modifier = Modifier.padding(padding)
            .fillMaxSize()) {
            items(servers) { server ->
                ServerItem(
                    server = server,
                    onDelete = { onRemoveServer(server) },
                    onSetActive = { onSetActiveServer(server) }
                )
            }
        }
    }
}

@Composable
fun ServerItem(server: Server, onDelete: () -> Unit, onSetActive: () -> Unit) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .padding(8.dp)
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Column {
                Text(server.address)
                if (server.isActive) {
                    Text("Активный", color = Color.Green)
                }
            }
            Row {
                Button(onClick = onSetActive) {
                    Text("Выбрать")
                }
                Spacer(modifier = Modifier.width(8.dp))
                IconButton(onClick = onDelete) {
                    Icon(Icons.Default.Delete, contentDescription = "Delete")
                }
            }
        }
    }
}