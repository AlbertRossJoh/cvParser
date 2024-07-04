'use client'
import {useEffect, useRef, useState} from "react";
import {LinkedList} from "../../misc/LinkedList";


export default function TerminalInput() {
    const [input, setInput] = useState('help');
    const [response, setResponse] = useState('');
    const [commandHis, setCommandHis] = useState([]);
    const [commandHisIdx, setCommandHisIdx] = useState(0);

    

    useEffect(() => {
        function handleKeypress(e){
            //console.log(e.code)
            //console.log(commandHis)
            if (e.code === "ArrowUp"){
                setInput(commandHis[commandHisIdx])
                if (commandHisIdx < commandHis.length-1){
                    setCommandHisIdx(val => val+1)    
                }
            } else if (e.code === "ArrowDown"){
                setInput(commandHis[commandHisIdx])
                if (commandHisIdx > 0){
                    setCommandHisIdx(val => val-1)
                }
            }
        }
        document.addEventListener('keydown', handleKeypress);
        return () =>{
            document.removeEventListener('keydown', handleKeypress);
        } 
    }, [commandHisIdx])
    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            commandHis.unshift(input);
            console.log(commandHis);
            const res = await fetch('http://localhost:5185/query', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ query: input }),
            });
            const data = JSON.stringify(await res.json(), null, 2);
            setInput("");
            setResponse(data);
        } catch (error) {
            console.error('Error:', error);
            setResponse('An error occurred');
        }
    };

    return (
        <div className="flex flex-col items-center justify-center min-h-screen bg-gray-900 break-words">
            <form onSubmit={handleSubmit} className="">
                <div className="flex items-center bg-black text-green-500 p-4 rounded-t shadow-lg w-160">
                    <span className="pr-2">$</span>
                    <input
                        type="text"
                        className="bg-black outline-none flex-1 text-green-500"
                        value={input}
                        onChange={(e) => setInput(e.target.value)}
                        autoFocus
                    />
                </div>
            </form>
            {response && (
                <div className="flex items-center bg-black text-green-500 p-4 shadow-lg w-160">
                    <pre className="break-words">{response}</pre>
                </div>
            )}
        </div>
    );
}
