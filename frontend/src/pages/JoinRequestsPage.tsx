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

function statusBadgeClass(status: string) {
  if (status === 'Joined') return 'badge badge-emerald'
  if (status === 'AwaitingPayment') return 'badge badge-amber'
  return 'badge badge-gray'
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
        waiveRefundConfirmation: refundChoice === 'WaiveRefund' ? waiveText : null,
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
    <main className="page page-wide">
      <div>
        <h1 className="page-title">My join requests</h1>
        <p className="page-subtitle">Theo dõi yêu cầu tham gia, hạn thanh toán và lượt đã join.</p>
      </div>

      {error ? <div className="alert-error mt-4">{error}</div> : null}

      {isLoading ? <div className="skeleton mt-5 h-28" aria-busy="true" /> : null}

      {!isLoading && requests.length === 0 ? (
        <section className="panel panel-pad mt-5">
          <p className="text-sm text-gray-600">You have not requested to join any play sessions yet.</p>
        </section>
      ) : null}

      <div className="mt-5 space-y-3">
        {requests.map((request) => (
          <article key={request.id} className="panel panel-pad">
            <div className="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
              <div>
                <span className={statusBadgeClass(request.status)}>{request.status}</span>
                <h2 className="mt-2 text-lg font-semibold text-gray-950">
                  {request.playSessionTitle}
                </h2>
                <p className="mt-1 text-sm text-gray-600">{request.courtName}</p>
                <p className="mt-2 text-sm text-gray-600">
                  Price: {currencyFormatter.format(request.pricePerPlayerVnd)}
                </p>
                {request.paymentDueAtUtc ? (
                  <p className="mt-1 text-sm text-gray-600">
                    Payment due: {new Date(request.paymentDueAtUtc).toLocaleString('vi-VN')}
                  </p>
                ) : null}
              </div>

              <div className="flex flex-wrap gap-2">
                {request.status === 'AwaitingPayment' ? (
                  <button
                    className="btn btn-primary"
                    disabled={submittingId === request.id}
                    onClick={() => void handlePay(request.id)}
                    type="button"
                  >
                    {submittingId === request.id ? 'Paying...' : 'Pay from wallet'}
                  </button>
                ) : null}

                {request.status === 'Joined' && paidParticipants[request.id] ? (
                  <button
                    className="btn btn-secondary"
                    disabled={submittingId === request.id}
                    onClick={() => setCancelTarget(request)}
                    type="button"
                  >
                    Cancel participation
                  </button>
                ) : null}
              </div>
            </div>
          </article>
        ))}
      </div>

      {cancelTarget ? (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-950/45 px-4">
          <section className="panel w-full max-w-lg p-5 shadow-lg">
            <h2 className="text-lg font-semibold text-gray-950">Cancel participation</h2>
            <p className="mt-2 text-sm text-gray-600">Session: {cancelTarget.playSessionTitle}</p>

            <div className="mt-4 space-y-3">
              <label className="flex gap-3 rounded-lg border border-gray-200 p-3 text-sm">
                <input
                  checked={refundChoice === 'StandardRefund'}
                  onChange={() => setRefundChoice('StandardRefund')}
                  type="radio"
                />
                <span>Refund 90%, host receives 10% cancellation fee.</span>
              </label>

              <label className="flex gap-3 rounded-lg border border-gray-200 p-3 text-sm">
                <input
                  checked={refundChoice === 'WaiveRefund'}
                  onChange={() => setRefundChoice('WaiveRefund')}
                  type="radio"
                />
                <span>No refund. Host receives the full paid amount.</span>
              </label>
            </div>

            <label className="label mt-4" htmlFor="reason">
              Reason
            </label>
            <textarea
              className="input min-h-20"
              id="reason"
              onChange={(event) => setReason(event.target.value)}
              value={reason}
            />

            {refundChoice === 'WaiveRefund' ? (
              <div className="mt-4 space-y-3 rounded-lg border border-red-200 bg-red-50 p-3">
                <label className="flex gap-2 text-sm text-red-900">
                  <input
                    checked={waiveChecked}
                    onChange={(event) => setWaiveChecked(event.target.checked)}
                    type="checkbox"
                  />
                  <span>I understand that I will not receive a refund.</span>
                </label>
                <input
                  className="input border-red-300"
                  onChange={(event) => setWaiveText(event.target.value)}
                  placeholder="Type KHONG HOAN TIEN"
                  value={waiveText}
                />
              </div>
            ) : null}

            <div className="mt-5 flex flex-wrap justify-end gap-2">
              <button className="btn btn-secondary" onClick={() => setCancelTarget(null)} type="button">
                Close
              </button>
              <button
                className="btn bg-red-700 text-white hover:bg-red-800 focus:ring-red-500"
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
