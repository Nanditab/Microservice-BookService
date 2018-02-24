

namespace BookService.Controllers
{
    #region -- Using --
    using BookService.Models;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Web.Http.Description;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using System;
    using BookService.DTO;
    #endregion
  [RoutePrefix("api/books")]
  [Route("api/books")]
    public class BooksController : ApiController
    {
        private BookServiceContext db = new BookServiceContext();
        //LINQ Expression
        private static readonly Expression<Func<Book, BookDTO>> AsBookDTO = x => new BookDTO
        {
            Title = x.Title,
            Genre = x.Genre,
            Author = x.Author.Name
        };

        private static readonly Expression<Func<Book, BookDetailDTO>> AsBookDetailDTO =
            x => new BookDetailDTO
            {
                 Title = x.Title,
                  Description = x.Description,
                   Genre = x.Genre,
                    Price = x.Price,
                     PublishDate = x.PublishDate,
                       Author = x.Author.Name
            };

        // GET: api/Books   
             public IQueryable<BookDTO> GetBooks()
        {
            return db.Books.Include(b => b.Author).Select(AsBookDTO);
        }

        // GET: api/Books/5
         [Route("{id:int}")]
        [ResponseType(typeof(BookDTO))]      
        public async Task<IHttpActionResult> GetBook(int id)
        {
            var book = await db.Books.Include(b => b.Author).Where(b => b.BookId == id).Select(AsBookDTO).FirstOrDefaultAsync();
            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        [Route("{id:int:min(1)}/Details")]
        [ResponseType(typeof(BookDetailDTO))]
        public async Task<IHttpActionResult> GetBookDetails(int id)
        {
            var bookDetail = await (from b in db.Books.Include(b => b.Author)
                                   where b.BookId == id
                                   select new BookDetailDTO
                                   {
                                       Title = b.Title,
                                       Description = b.Description,
                                       Genre = b.Genre,
                                       Price = b.Price,
                                       PublishDate = b.PublishDate,
                                       Author = b.Author.Name
                                   }).FirstOrDefaultAsync();
            if( bookDetail == null)
            {
                return NotFound();
            }
            return Ok(bookDetail);
              
        }

        [Route("{genre}")]
        [ResponseType(typeof(BookDTO))]
        public IQueryable<BookDTO> GetBooksByGenre(string genre)
        {
            return db.Books.Include(b => b.Author).Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase)).Select(AsBookDTO);
        }

        [Route("~/api/Author/{id:int}/Books")]
        public IEnumerable<BookDTO> GetBooksByAuthor(int id)
        {
            return db.Books.Include(b => b.Author).Where(b => b.AuthorId == id).Select(AsBookDTO).AsEnumerable();
        }

        [Route("date/{pubdate:datetime}")]
        public IEnumerable<BookDTO> GetBooksByPublishingDate(DateTime pubdate)
        {
            return db.Books.Include(b => b.Author).Where(b => DbFunctions.TruncateTime(b.PublishDate) == DbFunctions.TruncateTime(pubdate)).Select(AsBookDTO);
        }

        // PUT: api/Books/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutBook(int id, Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != book.BookId)
            {
                return BadRequest();
            }

            db.Entry(book).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Books
        [ResponseType(typeof(Book))]
        public async Task<IHttpActionResult> PostBook(Book book)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Books.Add(book);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = book.BookId }, book);
        }

        // DELETE: api/Books/5
        [ResponseType(typeof(Book))]
        public async Task<IHttpActionResult> DeleteBook(int id)
        {
            Book book = await db.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            db.Books.Remove(book);
            await db.SaveChangesAsync();

            return Ok(book);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BookExists(int id)
        {
            return db.Books.Count(e => e.BookId == id) > 0;
        }
    }
}