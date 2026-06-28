import { Route, Routes } from 'react-router-dom'
import { Navbar } from './components/Navbar'
import { CreatePlaySessionPage } from './pages/CreatePlaySessionPage'
import { DashboardPage } from './pages/DashboardPage'
import { FeedPage } from './pages/FeedPage'
import { HomePage } from './pages/HomePage'
import { LoginPage } from './pages/LoginPage'
import { PlaySessionDetailPage } from './pages/PlaySessionDetailPage'
import { RegisterPage } from './pages/RegisterPage'
import { ProtectedRoute } from './routes/ProtectedRoute'

function App() {
  return (
    <div className="flex min-h-screen flex-col">
      <Navbar />
      <Routes>
        <Route path="/" element={<HomePage />} />
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />
        <Route
          path="/feed"
          element={
            <ProtectedRoute>
              <FeedPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/play-sessions/:id"
          element={
            <ProtectedRoute>
              <PlaySessionDetailPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/play-sessions/create"
          element={
            <ProtectedRoute>
              <CreatePlaySessionPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/dashboard"
          element={
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </div>
  )
}

export default App
