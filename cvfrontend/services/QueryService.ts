
const endpoint = process.env.NEXT_PUBLIC_BACKENDURL+'/query';

export interface command{
    command: string,
    description: string,
    usage: string   
}

export interface legend{
    commands: command[],
    examples: string[]
}

export async function query(input:string): Promise<string> {
    const res = await fetch(endpoint, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ query: input }),
    });
    return JSON.stringify(await res.json(), null, 2);
}

export async function getLegend(): Promise<legend> {
    let res = await query('help');
    return JSON.parse(res);
}