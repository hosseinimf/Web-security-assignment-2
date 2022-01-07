#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WS_uppgift2.Data;
using WS_uppgift2.Models;

namespace WS_uppgift2.Controllers
{
    public class CommentsController : Controller
    {
        private readonly WS_uppgift2Context Db;

        public List<string> allowedTags { get; set; }

        public CommentsController(WS_uppgift2Context context)
        {
            Db = context;
            allowedTags = new List<string>() { "<b>", "</b>", "<i>", "</i>", "<u>", "</u>" };
        }

        // GET: Comments
        public async Task<IActionResult> Index()
        {
            return View(await Db.Comments.ToListAsync());
        }

        // GET: Comments/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comments = await Db.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
        }

        // GET: Comments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Comments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content")] Comments comments)
        {
            if (ModelState.IsValid)
            {
                comments.Id = Guid.NewGuid();
                comments.TimeStamp = DateTime.Now;
                string encodedContent = HttpUtility.HtmlEncode(comments.Content);
                foreach (var tag in allowedTags)
                {
                    string encodedTag = HttpUtility.HtmlEncode(tag);
                    encodedContent = encodedContent.Replace(encodedTag, tag);
                }
                comments.Content = encodedContent;

                Db.Add(comments);
                await Db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(comments);
        }

        // GET: Comments/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comments = await Db.Comments.FindAsync(id);
            if (comments == null)
            {
                return NotFound();
            }
            return View(comments);
        }

        // POST: Comments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,TimeStamp,Content")] Comments comments)
        {
            if (id != comments.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Db.Update(comments);
                    await Db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommentsExists(comments.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(comments);
        }

        // GET: Comments/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var comments = await Db.Comments
                .FirstOrDefaultAsync(m => m.Id == id);
            if (comments == null)
            {
                return NotFound();
            }

            return View(comments);
        }

        // POST: Comments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var comments = await Db.Comments.FindAsync(id);
            Db.Comments.Remove(comments);
            await Db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CommentsExists(Guid id)
        {
            return Db.Comments.Any(e => e.Id == id);
        }
    }
}
