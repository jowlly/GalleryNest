package com.example.owlnest

import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Build
import androidx.compose.material.icons.filled.Favorite
import androidx.compose.material.icons.filled.Send
import androidx.compose.material.icons.filled.Star
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.*
import androidx.compose.ui.graphics.*
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.platform.LocalContext
import androidx.navigation.NavController
import androidx.navigation.NavHostController
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState

@Composable
fun Navigator(navController: NavHostController){

    val context = LocalContext.current
    LaunchedEffect(Unit) {

        PhotoService.initialize(context)
    }

    Scaffold(
        bottomBar = { BottomNavigationBar(navController) }
    ) { paddingValues ->
        NavHostContainer(navController = navController, padding = paddingValues)
    }
}

sealed class Screen(val route: String, val title: String) {
    object Gallery : Screen("gallery", "Gallery")
    object Server : Screen("server", "Server")
    object Albums : Screen("albums", "Albums")
    object Source : Screen("source", "Source")
}



@Composable
fun NavHostContainer(
    navController: NavHostController,
    padding: PaddingValues
) {

    NavHost(
        navController = navController,

        // set the start destination as home
        startDestination = "gallery",

        // Set the padding provided by scaffold
        modifier = Modifier.padding(paddingValues = padding),

        builder = {

            composable("scanner") {
                QrScannerScreen(navController)
            }
            composable("gallery") {
                GalleryScreen(navController)
            }

            composable("albums") {
                GalleryScreen(navController)
            }

            composable("source") {
                SourceScreen(navController)
            }

            composable("server") {
                ServerGalleryScreen(navController)
            }
        })
}

@Composable
fun BottomNavigationBar(navController: NavController) {

    NavigationBar(

        // set background color
        containerColor = Color(0xFF927CD6)
    ) {

        // observe the backstack
        val navBackStackEntry by navController.currentBackStackEntryAsState()

        // observe current route to change the icon
        // color,label color when navigated
        val currentRoute = navBackStackEntry?.destination?.route

        // Bottom nav items we declared
        Constants.BottomNavItems.forEach { navItem ->

            // Place the bottom nav items
            NavigationBarItem(

                // it currentRoute is equal then its selected route
                selected = currentRoute == navItem.route,

                // navigate on click
                onClick = {
                    navController.navigate(navItem.route)
                },

                // Icon of navItem
                icon = {
                    Icon(imageVector = navItem.icon, contentDescription = navItem.label)
                },

                // label
                label = {
                    Text(text = navItem.label)
                },
                alwaysShowLabel = false,

                colors = NavigationBarItemDefaults.colors(
                    selectedIconColor = Color.White, // Icon color when selected
                    unselectedIconColor = Color.White, // Icon color when not selected
                    selectedTextColor = Color.White, // Label color when selected
                    indicatorColor = Color(0xFF6C52C9) // Highlight color for selected item
                )
            )
        }
    }
}

object Constants {
    val BottomNavItems = listOf(
        BottomNavItem(
            route = Screen.Gallery.route,
            label = Screen.Gallery.title,
            icon = Icons.Default.Favorite
        ),
        BottomNavItem(
            route = Screen.Server.route,
            label = Screen.Server.title,
            icon = Icons.Default.Send
        ),
        BottomNavItem(
            route = Screen.Albums.route,
            label = Screen.Albums.title,
            icon = Icons.Default.Star
        ),
        BottomNavItem(
            route = Screen.Source.route,
            label = Screen.Source.title,
            icon = Icons.Default.Build
        )
    )
}

data class BottomNavItem(
    val route: String,
    val label: String,
    val icon: ImageVector
)
