//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Media.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Movie
    {
        public Movie()
        {
            this.MovieRequest = new HashSet<MovieRequest>();
        }
    
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string Sinopsis { get; set; }
    
        public virtual ICollection<MovieRequest> MovieRequest { get; set; }
    }
}
