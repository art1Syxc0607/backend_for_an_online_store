import { Component, ErrorInfo, ReactNode } from 'react'

interface Props {
    children: ReactNode
    fallback?: ReactNode
}

interface State {
    hasError: boolean
    error?: Error
}

export default class ErrorBoundary extends Component<Props, State> {
    state: State = { hasError: false }

    static getDerivedStateFromError(error: Error): State {
        return { hasError: true, error }
    }

    componentDidCatch(error: Error, errorInfo: ErrorInfo) {
        console.error('ErrorBoundary caught:', error, errorInfo)
    }

    render() {
        if (this.state.hasError) {
            return this.props.fallback ?? (
                <div className="text-center py-20">
                    <h2 className="text-2xl font-bold text-red-600">
                        Chto-to poshlo ne tak
                    </h2>
                    <p className="text-gray-600 mt-2">
                        {this.state.error?.message ?? 'Neizvestnaya oshibka'}
                    </p>
                    <button
                        onClick={() => {
                            this.setState({ hasError: false, error: undefined })
                            window.location.reload()
                        }}
                        className="mt-4 px-6 py-2 bg-blue-500 text-white rounded-lg hover:bg-blue-600"
                    >
                        Obnovit stranitsu
                    </button>
                </div>
            )
        }

        return this.props.children
    }
}