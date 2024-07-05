
const endpoint = "http://localhost:8080/query"

export async function query(input:string){
    const res = await fetch('http://localhost:8080/query', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({ query: input }),
    });
    return JSON.stringify(await res.json(), null, 2);
}
