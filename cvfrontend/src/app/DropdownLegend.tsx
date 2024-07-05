'use client'
import {useEffect, useState} from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import {command, getLegend} from "../../services/QueryService";

const LegendBox = () => {
    const [isOpen, setIsOpen] = useState(false);
    const [commands, setCommands] = useState<command[]>([])
    const [examples, setExamples] = useState<string[]>([])

    
    useEffect(() => {
        async function getCommands(){
            let legend = await getLegend();
            setCommands(legend.commands);
            setExamples(legend.examples);
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
                        {commands?.map((cmd: command, index: any) => (
                            <li key={index}>
                                <strong>{cmd.command}</strong>: {cmd.description}
                                <br/>
                                <em>Usage:</em> <code>{cmd.usage}</code>
                            </li>
                        ))}
                    </ul>
                    <h3 className="mt-4 text-lg font-bold">Examples</h3>
                    <ul className="list-disc list-inside space-y-1">
                        {examples.map((example: string, index: any) => (
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
