import axios from 'axios'
import { useEffect, useState } from 'react'
import {
  confirmJoinRequestPayment,
  getMyJoinRequests,
} from '../api/joinRequests'
import { cancelParticipation } from '../api/participations'
import type { JoinRequest } from '../types/joinRequest'
import type { CancellationRefundChoice } from '../types/participation'

const currencyFormatter = new Intl.NumberFormat('vi-VN', {
  currency: 'VND',
  maximumFractionDigits: 0,
  style: 'currency',
})

function getErrorMessage(error: unknown) {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data
    if (typeof data === 'object' && data !== null && 'message' in data) {
      return String(data.message)
    }
  }

  return 'Request failed. Please try again.'
}

export function JoinRequestsPage() {
  const [requests, setRequests] = useState<JoinRequest[]>([])
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [submittingId, setSubmittingId] = useState('')
  const [paidParticipants, setPaidParticipants] = useState<Record<string, string>>({})
  const [cancelTarget, setCancelTarget] = useState<JoinRequest | null>(null)
  const [refundChoice, setRefundChoice] = useState<CancellationRefundChoice>('StandardRefund')
  const [reason, setReason] = useState('')
  const [waiveChecked, setWaiveChecked] = useState(false)
  const [waiveText, setWaiveText] = useState('')

  async function loadRequests() {
    setError('')
    const data = await getMyJoinRequests()
    setRequests(data)
  }

  useEffect(() => {
    let isMounted = true

    async function load() {
      try {
        const data = await getMyJoinRequests()
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

  async function handlePay(requestId: string) {
    setSubmittingId(requestId)
    setError('')

    try {
      const payment = await confirmJoinRequestPayment(requestId)
      setPaidParticipants((current) => ({
        ...current,
        [requestId]: payment.participantId,
      }))
      await loadRequests()
    } catch (paymentError) {
      setError(getErrorMessage(paymentError))
    } finally {
      setSubmittingId('')
    }
  }

  async function handleCancel() {
    if (!cancelTarget) return

    const participantId = paidParticipants[cancelTarget.id]
    if (!participantId) return

    setSubmittingId(cancelTarget.id)
    setError('')

    try {
      await cancelParticipation(participantId, {
        refundChoice,
        reason: reason || null,
        waiveRefundConfirmation:
          refundChoice === 'WaiveRefund' ? waiveText : null,
      })
      setCancelTarget(null)
      await loadRequests()
    } catch (cancelError) {
      setError(getErrorMessage(cancelError))
    } finally {
      setSubmittingId('')
    }
  }

  return (
    <main className="mx-auto w-full max-w-4xl flex-1 px-4 py-6 sm:px-6 lg:py-8">
      <h1 className="text-2xl font-semibold text-gray-950">My join requests</h1>

      {error ? (
        <div className="mt-4 rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      ) : null}

      {isLoading ? <p className="mt-4 text-sm text-gray-600">Loading requests...</p> : null}

      {!isLoading && requests.length === 0 ? (
        <p className="mt-4 rounded border border-gray-200 bg-white p-5 text-sm text-gray-600">
          You have not requested to join any play sessions yet.
        </p>
      ) : null}

      <div className="mt-5 space-y-3">
        {requests.map((request) => (
          <article key={request.id} className="rounded border border-gray-200 bg-white p-5">
            <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <p className="text-sm text-gray-500">{request.courtName}</p>
                <h2 className="mt-1 text-lg font-semibold text-gray-950">
                  {request.playSessionTitle}
                </h2>
                <p className="mt-2 text-sm text-gray-600">
                  Status: <span className="font-medium text-gray-900">{request.status}</span>
                </p>
                <p className="mt-1 text-sm text-gray-600">
                  Price: {currencyFormatter.format(request.pricePerPlayerVnd)}
                </p>
                {request.paymentDueAtUtc ? (
                  <p className="mt-1 text-sm text-gray-600">
                    Payment due: {new Date(request.paymentDueAtUtc).toLocaleString('vi-VN')}
                  </p>
                ) : null}
              </div>

              {request.status === 'AwaitingPayment' ? (
                <button
                  className="inline-flex items-center justify-center rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:bg-gray-300"
                  disabled={submittingId === request.id}
                  onClick={() => void handlePay(request.id)}
                  type="button"
                >
                  {submittingId === request.id ? 'Paying...' : 'Pay from wallet'}
                </button>
              ) : null}

              {request.status === 'Joined' && paidParticipants[request.id] ? (
                <button
                  className="inline-flex items-center justify-center rounded border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-800 hover:bg-gray-50 disabled:cursor-not-allowed disabled:bg-gray-100"
                  disabled={submittingId === request.id}
                  onClick={() => setCancelTarget(request)}
                  type="button"
                >
                  Cancel participation
                </button>
              ) : null}
            </div>
          </article>
        ))}
      </div>

      {cancelTarget ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 px-4">
          <section className="w-full max-w-lg rounded bg-white p-5 shadow-lg">
            <h2 className="text-lg font-semibold text-gray-950">Cancel participation</h2>
            <p className="mt-2 text-sm text-gray-600">
              Session: {cancelTarget.playSessionTitle}
            </p>

            <div className="mt-4 space-y-3">
              <label className="flex gap-3 rounded border border-gray-200 p-3 text-sm">
                <input
                  checked={refundChoice === 'StandardRefund'}
                  onChange={() => setRefundChoice('StandardRefund')}
                  type="radio"
                />
                <span>Refund 90%, host receives 10% cancellation fee.</span>
              </label>

              <label className="flex gap-3 rounded border border-gray-200 p-3 text-sm">
                <input
                  checked={refundChoice === 'WaiveRefund'}
                  onChange={() => setRefundChoice('WaiveRefund')}
                  type="radio"
                />
                <span>No refund. Host receives the full paid amount.</span>
              </label>
            </div>

            <label className="mt-4 block text-sm font-medium text-gray-700" htmlFor="reason">
              Reason
            </label>
            <textarea
              className="mt-1 min-h-20 w-full rounded border border-gray-300 px-3 py-2 text-sm"
              id="reason"
              onChange={(event) => setReason(event.target.value)}
              value={reason}
            />

            {refundChoice === 'WaiveRefund' ? (
              <div className="mt-4 space-y-3 rounded border border-red-200 bg-red-50 p-3">
                <label className="flex gap-2 text-sm text-red-900">
                  <input
                    checked={waiveChecked}
                    onChange={(event) => setWaiveChecked(event.target.checked)}
                    type="checkbox"
                  />
                  <span>I understand that I will not receive a refund.</span>
                </label>
                <input
                  className="w-full rounded border border-red-300 px-3 py-2 text-sm"
                  onChange={(event) => setWaiveText(event.target.value)}
                  placeholder="Type KHONG HOAN TIEN"
                  value={waiveText}
                />
              </div>
            ) : null}

            <div className="mt-5 flex flex-wrap justify-end gap-2">
              <button
                className="rounded border border-gray-300 bg-white px-4 py-2 text-sm font-medium text-gray-800 hover:bg-gray-50"
                onClick={() => setCancelTarget(null)}
                type="button"
              >
                Close
              </button>
              <button
                className="rounded bg-red-700 px-4 py-2 text-sm font-medium text-white hover:bg-red-800 disabled:cursor-not-allowed disabled:bg-gray-300"
                disabled={
                  submittingId === cancelTarget.id ||
                  (refundChoice === 'WaiveRefund' &&
                    (!waiveChecked || waiveText !== 'KHONG HOAN TIEN'))
                }
                onClick={() => void handleCancel()}
                type="button"
              >
                Confirm cancel
              </button>
            </div>
          </section>
        </div>
      ) : null}
    </main>
  )
}
