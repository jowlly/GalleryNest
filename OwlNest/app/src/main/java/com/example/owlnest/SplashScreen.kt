import android.content.Context
import androidx.compose.foundation.layout.Box
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.CircularProgressIndicator
import androidx.compose.runtime.Composable
import androidx.compose.runtime.LaunchedEffect
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.platform.LocalContext
import androidx.navigation.NavController
import com.example.owlnest.PhotoService

@Composable
fun SplashScreen(navController: NavController) {
    val context = LocalContext.current

    LaunchedEffect(Unit) {
        val prefs = context.getSharedPreferences("app_prefs", Context.MODE_PRIVATE)
        val savedUrl = prefs.getString("server_url", null)

        val destination = if (!savedUrl.isNullOrEmpty()) {
            PhotoService.initialize(context)
            if (PhotoService.checkServerAvailability()) "gallery" else "scanner"
        } else {
            "scanner"
        }

        navController.navigate(destination) {
            popUpTo("splash") { inclusive = true }
        }
    }

    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = Alignment.Center
    ) {
        CircularProgressIndicator()
    }
}