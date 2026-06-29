import axios from 'axios'
import { useEffect, useState } from 'react'
import {
  getNotifications,
  markNotificationAsRead,
} from '../api/notifications'
import type { Notification } from '../types/notification'

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data
    if (typeof data === 'object' && data !== null && 'message' in data) {
      return String(data.message)
    }
  }

  return 'Notification request failed. Please try again.'
}

export function NotificationsPage() {
  const [notifications, setNotifications] = useState<Notification[]>([])
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)

  async function loadNotifications() {
    setError('')
    setNotifications(await getNotifications())
  }

  useEffect(() => {
    let isMounted = true

    async function load() {
      try {
        const data = await getNotifications()
        if (isMounted) setNotifications(data)
      } catch (loadError) {
        if (isMounted) setError(getErrorMessage(loadError))
      } finally {
        if (isMounted) setIsLoading(false)
      }
    }

    void load()

    return () => {
      isMounted = false
    }
  }, [])

  async function handleRead(notificationId: string) {
    try {
      await markNotificationAsRead(notificationId)
      await loadNotifications()
    } catch (readError) {
      setError(getErrorMessage(readError))
    }
  }

  return (
    <main className="mx-auto w-full max-w-4xl flex-1 px-4 py-6 sm:px-6 lg:py-8">
      <h1 className="text-2xl font-semibold text-gray-950">Notifications</h1>

      {error ? (
        <div className="mt-4 rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      ) : null}

      {isLoading ? <p className="mt-4 text-sm text-gray-600">Loading notifications...</p> : null}

      {!isLoading && notifications.length === 0 ? (
        <p className="mt-4 rounded border border-gray-200 bg-white p-5 text-sm text-gray-600">
          No notifications yet.
        </p>
      ) : null}

      <div className="mt-5 space-y-3">
        {notifications.map((notification) => (
          <article key={notification.id} className="rounded border border-gray-200 bg-white p-5">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <p className="text-xs font-medium uppercase text-gray-500">{notification.type}</p>
                <h2 className="mt-1 text-lg font-semibold text-gray-950">{notification.title}</h2>
                <p className="mt-2 text-sm text-gray-600">{notification.message}</p>
              </div>

              {!notification.isRead ? (
                <button
                  className="rounded border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-800 hover:bg-gray-50"
                  onClick={() => void handleRead(notification.id)}
                  type="button"
                >
                  Mark read
                </button>
              ) : (
                <span className="text-sm text-gray-500">Read</span>
              )}
            </div>
          </article>
        ))}
      </div>
    </main>
  )
}
