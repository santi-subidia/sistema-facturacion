import React, { Suspense } from 'react'
import { BrowserRouter, Routes, Route } from 'react-router-dom'
import routes from './routes'
import LoadingFallback from './components/routing/LoadingFallback'
import ErrorBoundary from './components/shared/ErrorBoundary'
import { AuthProvider } from './context/AuthContext'
import { ConfirmProvider } from './context/ConfirmContext'
import { CajaProvider } from './context/CajaContext'

function App() {
  return (
    <ErrorBoundary>
      <AuthProvider>
        <ConfirmProvider>
          <CajaProvider>
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
          </CajaProvider>
        </ConfirmProvider>
      </AuthProvider>
    </ErrorBoundary>
  )
}

export default App
