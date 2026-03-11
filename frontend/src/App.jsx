import React, { Suspense } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import routes from './routes'
import LoadingFallback from './components/routing/LoadingFallback'
import ErrorBoundary from './components/shared/ErrorBoundary'
import { AuthProvider } from './context/AuthContext'

function App() {
  return (
    <ErrorBoundary>
      <AuthProvider>
        <BrowserRouter>
          <Suspense fallback={<LoadingFallback />}>
            <Routes>
              {routes.map((route, index) => (
                <Route
                  key={index}
                  path={route.path}
                  element={route.element}
                >
                  {route.children?.map((child, childIndex) => (
                    <Route
                      key={childIndex}
                      path={child.path}
                      index={child.index}
                      element={child.element}
                    />
                  ))}
                </Route>
              ))}
            </Routes>
          </Suspense>
        </BrowserRouter>
      </AuthProvider>
    </ErrorBoundary>
  )
}

export default App
