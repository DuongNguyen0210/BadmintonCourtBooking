import axios from 'axios'
import { useEffect, useState } from 'react'
import {
  approveJoinRequest,
  getHostJoinRequests,
  rejectJoinRequest,
} from '../api/joinRequests'
import type { JoinRequest } from '../types/joinRequest'

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data
    if (typeof data === 'object' && data !== null && 'message' in data) {
      return String(data.message)
    }
  }

  return 'Request failed. Please try again.'
}

export function HostJoinRequestsPage() {
  const [requests, setRequests] = useState<JoinRequest[]>([])
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [submittingId, setSubmittingId] = useState('')

  async function loadRequests() {
    setError('')
    const data = await getHostJoinRequests('PendingHostApproval')
    setRequests(data)
  }

  useEffect(() => {
    let isMounted = true

    async function load() {
      try {
        const data = await getHostJoinRequests('PendingHostApproval')
        if (isMounted) setRequests(data)
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

  async function handleApprove(requestId: string) {
    setSubmittingId(requestId)
    try {
      await approveJoinRequest(requestId)
      await loadRequests()
    } catch (approveError) {
      setError(getErrorMessage(approveError))
    } finally {
      setSubmittingId('')
    }
  }

  async function handleReject(requestId: string) {
    setSubmittingId(requestId)
    try {
      await rejectJoinRequest(requestId)
      await loadRequests()
    } catch (rejectError) {
      setError(getErrorMessage(rejectError))
    } finally {
      setSubmittingId('')
    }
  }

  return (
    <main className="page page-wide">
      <div>
        <h1 className="page-title">Host join requests</h1>
        <p className="page-subtitle">Duyệt hoặc từ chối các yêu cầu đang chờ của bài đăng bạn tạo.</p>
      </div>

      {error ? <div className="alert-error mt-4">{error}</div> : null}

      {isLoading ? <div className="skeleton mt-5 h-28" aria-busy="true" /> : null}

      {!isLoading && requests.length === 0 ? (
        <section className="panel panel-pad mt-5">
          <p className="text-sm text-gray-600">No pending requests.</p>
        </section>
      ) : null}

      <div className="mt-5 space-y-3">
        {requests.map((request) => (
          <article key={request.id} className="panel panel-pad">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <span className="badge badge-amber">Pending approval</span>
                <h2 className="mt-2 text-lg font-semibold text-gray-950">
                  {request.playSessionTitle}
                </h2>
                <p className="mt-1 text-sm text-gray-600">{request.courtName}</p>
                <p className="mt-2 text-sm text-gray-600">
                  Guest: <span className="font-medium text-gray-900">{request.guestName}</span>
                </p>
              </div>

              <div className="flex flex-wrap gap-2">
                <button
                  className="btn btn-primary"
                  disabled={submittingId === request.id}
                  onClick={() => void handleApprove(request.id)}
                  type="button"
                >
                  Approve
                </button>
                <button
                  className="btn btn-secondary"
                  disabled={submittingId === request.id}
                  onClick={() => void handleReject(request.id)}
                  type="button"
                >
                  Reject
                </button>
              </div>
            </div>
          </article>
        ))}
      </div>
    </main>
  )
}
