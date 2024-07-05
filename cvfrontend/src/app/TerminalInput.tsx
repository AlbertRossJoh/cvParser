'use client'
import {ChangeEvent, useEffect, useState} from "react";
import {query} from "../../services/QueryService";
import SyntaxHighlighter from "react-syntax-highlighter";
import {nord} from "react-syntax-highlighter/dist/cjs/styles/hljs";


export default function TerminalInput() {
    const [input, setInput] = useState('help');
    const [response, setResponse] = useState('');
    const [commandHis, setCommandHis] = useState<string[]>([]);
    const [commandHisIdx, setCommandHisIdx] = useState(0);

    useEffect(() => {
        function handleKeypress(e:any){
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
    const handleSubmit = async (e: any) => {
        e.preventDefault();
        try {
            commandHis.unshift(input);
            const data = await query(input);
            setInput("");
            setResponse(data);
        } catch (error) {
            console.error('Error:', error);
            setResponse('An error occurred');
        }
        setCommandHisIdx(0);
    };

    return (
        <div>
            <form onSubmit={handleSubmit} className="">
                <div className="flex items-center bg-black text-green-500 p-4 rounded shadow-lg w-160 caret-underscore">
                    <span className="pr-2">$</span>
                    <input
                        type="text"
                        className="bg-black outline-none flex-1 text-green-500 caret-green-500 caret-block"
                        value={input}
                        onChange={(e: ChangeEvent<HTMLInputElement>) => setInput(e.target.value)}
                        autoFocus
                    />
                </div>
            </form>
            {response && (
                <div className="w-160">
                    <SyntaxHighlighter language='json' style={nord} wrapLines={true} wrapLongLines={true} >
                        {response}
                    </SyntaxHighlighter>
                </div>
            )}
        </div>
    );
}
