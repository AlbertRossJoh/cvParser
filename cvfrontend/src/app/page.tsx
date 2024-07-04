import Image from "next/image";
import TerminalInput from "@/app/TerminalInput";
import LegendDropdown from "@/app/DropdownLegend";

export default function Home() {
    
  return (
      <main className="flex min-h-screen flex-col items-center justify-between p-24 bg-gray-900">
          <LegendDropdown/>
          <TerminalInput/>
      </main>
  );
}
