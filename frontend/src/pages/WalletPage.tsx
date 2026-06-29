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
    <main className="mx-auto w-full max-w-4xl flex-1 px-4 py-6 sm:px-6 lg:py-8">
      <h1 className="text-2xl font-semibold text-gray-950">Wallet</h1>

      {error ? (
        <div className="mt-4 rounded border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-800">
          {error}
        </div>
      ) : null}

      {isLoading ? <p className="mt-4 text-sm text-gray-600">Loading wallet...</p> : null}

      {wallet ? (
        <section className="mt-5 grid grid-cols-1 gap-3 sm:grid-cols-2">
          <div className="rounded border border-gray-200 bg-white p-5">
            <p className="text-sm text-gray-500">Available</p>
            <p className="mt-2 text-2xl font-semibold text-gray-950">
              {currencyFormatter.format(wallet.availableBalanceVnd)}
            </p>
          </div>
          <div className="rounded border border-gray-200 bg-white p-5">
            <p className="text-sm text-gray-500">Held</p>
            <p className="mt-2 text-2xl font-semibold text-gray-950">
              {currencyFormatter.format(wallet.heldBalanceVnd)}
            </p>
          </div>
        </section>
      ) : null}

      {import.meta.env.DEV ? (
        <section className="mt-5 rounded border border-gray-200 bg-white p-5">
          <h2 className="text-lg font-semibold text-gray-950">Development top-up</h2>
          <div className="mt-3 flex flex-col gap-3 sm:flex-row">
            <input
              className="rounded border border-gray-300 px-3 py-2 text-sm"
              min={1}
              onChange={(event) => setAmountVnd(Number(event.target.value))}
              type="number"
              value={amountVnd}
            />
            <button
              className="rounded bg-emerald-700 px-4 py-2 text-sm font-medium text-white hover:bg-emerald-800 disabled:cursor-not-allowed disabled:bg-gray-300"
              disabled={isSubmitting || amountVnd <= 0}
              onClick={() => void handleTopUp()}
              type="button"
            >
              {isSubmitting ? 'Adding...' : 'Add balance'}
            </button>
          </div>
        </section>
      ) : null}

      <section className="mt-5 rounded border border-gray-200 bg-white p-5">
        <h2 className="text-lg font-semibold text-gray-950">Transactions</h2>
        <div className="mt-3 divide-y divide-gray-100">
          {transactions.length === 0 ? (
            <p className="py-3 text-sm text-gray-600">No transactions yet.</p>
          ) : null}
          {transactions.map((transaction) => (
            <div key={transaction.id} className="py-3 text-sm">
              <div className="flex items-center justify-between gap-3">
                <span className="font-medium text-gray-900">{transaction.type}</span>
                <span>{currencyFormatter.format(transaction.amountVnd)}</span>
              </div>
              <p className="mt-1 text-gray-600">{transaction.description}</p>
            </div>
          ))}
        </div>
      </section>
    </main>
  )
}
