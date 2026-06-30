import axios from 'axios'
import { useEffect, useState } from 'react'
import {
  getWallet,
  getWalletTransactions,
  topUpDevelopmentWallet,
} from '../api/wallet'
import type { WalletResponse, WalletTransaction } from '../types/wallet'

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

  return 'Wallet request failed. Please try again.'
}

export function WalletPage() {
  const [wallet, setWallet] = useState<WalletResponse | null>(null)
  const [transactions, setTransactions] = useState<WalletTransaction[]>([])
  const [amountVnd, setAmountVnd] = useState(200000)
  const [error, setError] = useState('')
  const [isLoading, setIsLoading] = useState(true)
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function loadWallet() {
    setError('')
    const [walletData, transactionData] = await Promise.all([
      getWallet(),
      getWalletTransactions(),
    ])
    setWallet(walletData)
    setTransactions(transactionData)
  }

  useEffect(() => {
    let isMounted = true

    async function load() {
      try {
        const [walletData, transactionData] = await Promise.all([
          getWallet(),
          getWalletTransactions(),
        ])
        if (isMounted) {
          setWallet(walletData)
          setTransactions(transactionData)
        }
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

  async function handleTopUp() {
    setIsSubmitting(true)
    setError('')
    try {
      await topUpDevelopmentWallet({ amountVnd })
      await loadWallet()
    } catch (topUpError) {
      setError(getErrorMessage(topUpError))
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <main className="page page-wide">
      <div>
        <h1 className="page-title">Wallet</h1>
        <p className="page-subtitle">Theo dõi số dư khả dụng, tiền đang giữ và lịch sử giao dịch.</p>
      </div>

      {error ? <div className="alert-error mt-4">{error}</div> : null}

      {isLoading ? (
        <div className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-2" aria-busy="true">
          <div className="skeleton h-28" />
          <div className="skeleton h-28" />
        </div>
      ) : null}

      {wallet ? (
        <section className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div className="metric">
            <p className="metric-label">Available</p>
            <p className="metric-value">{currencyFormatter.format(wallet.availableBalanceVnd)}</p>
          </div>
          <div className="metric">
            <p className="metric-label">Held</p>
            <p className="metric-value">{currencyFormatter.format(wallet.heldBalanceVnd)}</p>
          </div>
        </section>
      ) : null}

      {import.meta.env.DEV ? (
        <section className="panel panel-pad mt-5">
          <div className="flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
              <h2 className="text-lg font-semibold text-gray-950">Development top-up</h2>
              <p className="mt-1 text-sm text-gray-600">Chỉ dùng cho môi trường development.</p>
            </div>
            <div className="flex flex-col gap-3 sm:flex-row">
              <input
                className="input mt-0 sm:w-48"
                min={1}
                onChange={(event) => setAmountVnd(Number(event.target.value))}
                type="number"
                value={amountVnd}
              />
              <button
                className="btn btn-primary"
                disabled={isSubmitting || amountVnd <= 0}
                onClick={() => void handleTopUp()}
                type="button"
              >
                {isSubmitting ? 'Adding...' : 'Add balance'}
              </button>
            </div>
          </div>
        </section>
      ) : null}

      <section className="panel panel-pad mt-5">
        <div className="flex items-center justify-between gap-3">
          <h2 className="text-lg font-semibold text-gray-950">Transactions</h2>
          <span className="badge badge-gray">{transactions.length}</span>
        </div>

        <div className="mt-3 divide-y divide-gray-100">
          {transactions.length === 0 ? (
            <p className="py-6 text-sm text-gray-600">No transactions yet.</p>
          ) : null}
          {transactions.map((transaction) => (
            <div key={transaction.id} className="py-3 text-sm">
              <div className="flex items-center justify-between gap-3">
                <span className="font-semibold text-gray-900">{transaction.type}</span>
                <span className="font-medium text-gray-950">
                  {currencyFormatter.format(transaction.amountVnd)}
                </span>
              </div>
              <p className="mt-1 text-gray-600">{transaction.description}</p>
            </div>
          ))}
        </div>
      </section>
    </main>
  )
}
