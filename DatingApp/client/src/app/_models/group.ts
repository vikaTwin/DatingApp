export interface Group {
    name: string;
    connections: Connection[];
}

interface Connection {
    connetctionId: string;
    username: string;
}