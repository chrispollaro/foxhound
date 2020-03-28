namespace FoxHound.App.Domain

open System
open System.Collections.Generic

type public Post(owner: string) =
    member this.BlogId with get() = 0 
    member this.Owner with get() = owner 
    member this.CreatedDate with get() = DateTime.Now
    
    abstract Posts : IList<Post> with get, set
    
    [<DefaultValue>]
    val mutable private _Posts : IList<Post>
    default this.Posts with get() = this._Posts
                            and set(v) = this._Posts <- v
