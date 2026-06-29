import { Route, Routes } from 'react-router-dom'
import { Navbar } from './components/Navbar'
import { CreatePlaySessionPage } from './pages/CreatePlaySessionPage'
import { DashboardPage } from './pages/DashboardPage'
import { EditPlaySessionPage } from './pages/EditPlaySessionPage'
import { FeedPage } from './pages/FeedPage'
import { HostJoinRequestsPage } from './pages/HostJoinRequestsPage'
import { HomePage } from './pages/HomePage'
import { JoinRequestsPage } from './pages/JoinRequestsPage'
import { LoginPage } from './pages/LoginPage'
import { NotificationsPage } from './pages/NotificationsPage'
import { PlaySessionDetailPage } from './pages/PlaySessionDetailPage'
import { RegisterPage } from './pages/RegisterPage'
import { WalletPage } from './pages/WalletPage'
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
          path="/play-sessions/:id/edit"
          element={
            <ProtectedRoute>
              <EditPlaySessionPage />
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
          path="/join-requests"
          element={
            <ProtectedRoute>
              <JoinRequestsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/host/join-requests"
          element={
            <ProtectedRoute>
              <HostJoinRequestsPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/wallet"
          element={
            <ProtectedRoute>
              <WalletPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/notifications"
          element={
            <ProtectedRoute>
              <NotificationsPage />
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
