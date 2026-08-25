import { useRevealAll } from "./hooks";
import Nav from "./components/Nav";
import Hero from "./components/Hero";
import Ticker from "./components/Ticker";
import HybridLoop from "./components/HybridLoop";
import Calculator from "./components/Calculator";
import Agents from "./components/Agents";
import Metrics from "./components/Metrics";
import Social from "./components/Social";
import Pricing from "./components/Pricing";
import Faq from "./components/Faq";
import Footer from "./components/Footer";

export default function App() {
  useRevealAll();

  return (
    <div className="relative min-h-screen">
      {/* ambient layers */}
      <div className="bg-grid" aria-hidden="true" />
      <div className="bg-tint" aria-hidden="true" />
      <div className="noise" aria-hidden="true" />

      <Nav />
      <main>
        <Hero />
        <Ticker />
        <HybridLoop />
        <Calculator />
        <Agents />
        <Metrics />
        <Social />
        <Pricing />
        <Faq />
      </main>
      <Footer />
    </div>
  );
}
