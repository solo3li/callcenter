import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { createBrowserRouter, RouterProvider } from "react-router-dom";
import "./index.css";
import LandingPage from "./App";
import DashboardLayout from "./dashboard/DashboardLayout";
import LiveBoard from "./dashboard/pages/LiveBoard";
import AgentRoster from "./dashboard/pages/AgentRoster";
import Analytics from "./dashboard/pages/Analytics";
import CallHistory from "./dashboard/pages/CallHistory";
import CallDetail from "./dashboard/pages/CallDetail";

import ApiStatus from "./components/ApiStatus";

const router = createBrowserRouter([
  {
    path: "/",
    element: <LandingPage />,
  },
  {
    path: "/dashboard",
    element: <DashboardLayout />,
    children: [
      { index: true, element: <LiveBoard /> },
      { path: "live", element: <LiveBoard /> },
      { path: "roster", element: <AgentRoster /> },
      { path: "analytics", element: <Analytics /> },
      { path: "history", element: <CallHistory /> },
      { path: "call/:id", element: <CallDetail /> },
    ],
  },
]);

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <ApiStatus />
    <RouterProvider router={router} />
  </StrictMode>
);