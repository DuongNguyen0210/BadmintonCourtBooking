import axios from 'axios'
import { useEffect, useMemo, useState } from 'react'
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
  const unreadCount = useMemo(
    () => notifications.filter((notification) => !notification.isRead).length,
    [notifications],
  )

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
    <main className="page page-wide">
      <div className="flex flex-col gap-2 sm:flex-row sm:items-end sm:justify-between">
        <div>
          <h1 className="page-title">Notifications</h1>
          <p className="page-subtitle">Theo dõi duyệt kèo, thanh toán và hủy tham gia.</p>
        </div>
        <span className="badge badge-emerald">{unreadCount} unread</span>
      </div>

      {error ? <div className="alert-error mt-4">{error}</div> : null}

      {isLoading ? <div className="skeleton mt-5 h-28" aria-busy="true" /> : null}

      {!isLoading && notifications.length === 0 ? (
        <section className="panel panel-pad mt-5">
          <p className="text-sm text-gray-600">No notifications yet.</p>
        </section>
      ) : null}

      <div className="mt-5 space-y-3">
        {notifications.map((notification) => (
          <article key={notification.id} className="panel panel-pad">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <span className={notification.isRead ? 'badge badge-gray' : 'badge badge-emerald'}>
                  {notification.type}
                </span>
                <h2 className="mt-2 text-lg font-semibold text-gray-950">{notification.title}</h2>
                <p className="mt-2 text-sm leading-6 text-gray-600">{notification.message}</p>
              </div>

              {!notification.isRead ? (
                <button
                  className="btn btn-secondary"
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
