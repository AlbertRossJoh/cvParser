
interface LinkedList<T> {
    value : T,
    next: LinkedList<T>
}
export class LinkedList<T> {
    value?: T;
    next?: LinkedList<T>;
    prev?: LinkedList<T>;
    constructor(value?: T) {
        if (value) this.value = value;
    }
   
    public PushBack(item: T): LinkedList<T>{
        if (this.value === null) {
            this.value = item;
            return this;
        }
        let next =  new LinkedList<T>(item);
        if (this.next){
            let curr = this.next;
            while (curr.next){
                curr = curr.next;
            }
            curr.next = next;
            next.prev = curr;
        } else {
            this.next = next
            next.prev = this;
        }
        return this.next;
    }
    public PushFront(item: T): LinkedList<T>{
        if (this.value === null) {
            this.value = item;
            return this;
        }
        if (this.prev){
            let curr = this.prev;
            while (curr.prev){
                curr = curr.prev;
            }
            curr.prev = new LinkedList<T>(item);
            curr.prev!.next = curr;
        } else {
            this.prev = new LinkedList<T>(item)
            this.prev.next = this;
        }
        return this.prev;
    }
}
