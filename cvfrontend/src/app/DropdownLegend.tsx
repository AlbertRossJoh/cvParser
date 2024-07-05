'use client'
import {useCallback, useEffect, useState} from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './../components/ui/card';
import { Button } from './../components/ui/button';
import SyntaxHighlighter from "react-syntax-highlighter";

const LegendBox = () => {
    const [isOpen, setIsOpen] = useState(false);
    const [commands, setCommands] = useState<any>([])
    const [examples, setExamples] = useState<any>([])

    
    useEffect(() => {
        async function getCommands(){
            const res = await fetch('http://localhost:8080/query', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({query: 'help'}),
            });
            const data: any = await res.json();
            
            setCommands(data.commands);
            setExamples(data.examples);
        }
        getCommands()
        return()=>{};
    }, [])


    return (
        <div className="absolute top-4 right-0 w-80">
            <Button onClick={() => setIsOpen(!isOpen)} variant="outline">
                {isOpen ? 'Hide Commands' : 'Show Commands'}
            </Button>
            <Card
                className={`transform transition-all duration-300 mt-1 bg-slate-50 ${isOpen ? 'max-h-full opacity-100' : 'max-h-0 opacity-0 overflow-hidden'}`}>
                <CardHeader>
                    <CardTitle className="text-lg font-bold mb-2">Commands</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="mt-4 text-lg font-bold">All commands can be chained</div>
                    <ul className="space-y-2">
                        {commands?.map((cmd: {command: string, description: string, usage: string}, index: any) => (
                            <li key={index}>
                                <strong>{cmd.command}</strong>: {cmd.description}
                                <br/>
                                <em>Usage:</em> <code>{cmd.usage}</code>
                            </li>
                        ))}
                    </ul>
                    <h3 className="mt-4 text-lg font-bold">Examples</h3>
                    <ul className="list-disc list-inside space-y-1">
                        {examples.map((example: string[], index: any) => (
                            <li key={index}>
                                <code>{example}</code>
                            </li>
                        ))}
                    </ul>
                </CardContent>
            </Card>
        </div>
    );
};

export default LegendBox;
