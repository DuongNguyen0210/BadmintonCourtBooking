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
    <main className="mx-auto w-full max-w-4xl flex-1 px-4 py-6 sm:px-6 lg:py-8">
      <h1 className="text-2xl font-semibold text-gray-950">Host join requests</h1>

      {error ? (
        <div className="mt-4 rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      ) : null}

      {isLoading ? <p className="mt-4 text-sm text-gray-600">Loading requests...</p> : null}

      {!isLoading && requests.length === 0 ? (
        <p className="mt-4 rounded border border-gray-200 bg-white p-5 text-sm text-gray-600">
          No pending requests.
        </p>
      ) : null}

      <div className="mt-5 space-y-3">
        {requests.map((request) => (
          <article key={request.id} className="rounded border border-gray-200 bg-white p-5">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <h2 className="text-lg font-semibold text-gray-950">{request.playSessionTitle}</h2>
                <p className="mt-1 text-sm text-gray-600">{request.courtName}</p>
                <p className="mt-2 text-sm text-gray-600">
                  Guest: <span className="font-medium text-gray-900">{request.guestName}</span>
                </p>
              </div>

              <div className="flex flex-wrap gap-2">
                <button
                  className="rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:bg-gray-300"
                  disabled={submittingId === request.id}
                  onClick={() => void handleApprove(request.id)}
                  type="button"
                >
                  Approve
                </button>
                <button
                  className="rounded border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-800 hover:bg-gray-50 disabled:cursor-not-allowed disabled:bg-gray-100"
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
