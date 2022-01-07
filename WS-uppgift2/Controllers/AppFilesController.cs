#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using WS_uppgift2.Data;
using WS_uppgift2.Models;
using WS_uppgift2.Utilities;
using System.Net.Mime;
using Microsoft.Net.Http.Headers;

namespace WS_uppgift2.Controllers
{
    public class AppFilesController : Controller
    {
        private readonly WS_uppgift2Context Db;
        private readonly long fileSizeLimit = 10 * 1048576;
        private readonly string[] permittedExtensions = { ".jpg" };

        public AppFilesController(WS_uppgift2Context context)
        {
            Db = context;
        }


        //------------------------------------------------------upload------------
        [HttpPost]
        [Route(nameof(UploadFile))]
        public async Task<IActionResult> UploadFile()
        {
            var theWebRequest = HttpContext.Request;

            // validation of Content-Type
            // 1. first, it must be a form-data request
            // 2. a boundary should be found in the Content-Type
            if (!theWebRequest.HasFormContentType ||
                !MediaTypeHeaderValue.TryParse(theWebRequest.ContentType, out var theMediaTypeHeader) ||
                string.IsNullOrEmpty(theMediaTypeHeader.Boundary.Value))
            {
                return new UnsupportedMediaTypeResult();
            }

            var reader = new MultipartReader(theMediaTypeHeader.Boundary.Value, theWebRequest.Body);
            var section = await reader.ReadNextSectionAsync();

            // This sample try to get the first file from request and save it
            // Make changes according to your needs in actual use
            while (section != null)
            {
                var DoesItHaveContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition,
                    out var theContentDisposition);

                if (DoesItHaveContentDispositionHeader && theContentDisposition.DispositionType.Equals("form-data") &&
                    !string.IsNullOrEmpty(theContentDisposition.FileName.Value))
                {
                    // Don't trust any file name, file extension, and file data from the request unless you trust them completely
                    // Otherwise, it is very likely to cause problems such as virus uploading, disk filling, etc
                    // In short, it is necessary to restrict and verify the upload
                    // Here, we just use the temporary folder and a random file name

                    AppFile appFile = new AppFile();
                    appFile.UntrustedName = HttpUtility.HtmlEncode(theContentDisposition.FileName.Value);
                    appFile.TimeStamp = DateTime.UtcNow;

                    appFile.Content = await FileHelpers.ProcessStreamedFile(section, theContentDisposition,
                                      ModelState, permittedExtensions, fileSizeLimit);

                    if (appFile.Content.Length == 0)
                    {
                        return RedirectToAction("Index", "AppFiles");
                    }
                    appFile.Size = appFile.Content.Length;

                    

                    await Db.AppFile.AddAsync(appFile);
                    await Db.SaveChangesAsync();

                    return RedirectToAction("Index", "AppFiles");

                }

                section = await reader.ReadNextSectionAsync();
            }

            // If the code runs to this location, it means that no files have been saved
            return BadRequest("No files data in the request.");
        }
        //------------------------------------------------------upload end------------


        //------------------------------------------------------Download------------
        public async Task<IActionResult> Download(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicationFile = await Db.AppFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (applicationFile == null)
            {
                return NotFound();
            }

            return File(applicationFile.Content, MediaTypeNames.Application.Octet, applicationFile.UntrustedName);
        }
        //------------------------------------------------------Download end------------














        // GET: AppFiles
        public async Task<IActionResult> Index()
        {
            return View(await Db.AppFile.ToListAsync());
        }

        // GET: AppFiles/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appFile = await Db.AppFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appFile == null)
            {
                return NotFound();
            }

            return View(appFile);
        }

        // GET: AppFiles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AppFiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UntrustedName,TimeStamp,Size,Content")] AppFile appFile)
        {
            if (ModelState.IsValid)
            {
                appFile.Id = Guid.NewGuid();
                Db.Add(appFile);
                await Db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(appFile);
        }

        // GET: AppFiles/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appFile = await Db.AppFile.FindAsync(id);
            if (appFile == null)
            {
                return NotFound();
            }
            return View(appFile);
        }

        // POST: AppFiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,UntrustedName,TimeStamp,Size,Content")] AppFile appFile)
        {
            if (id != appFile.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Db.Update(appFile);
                    await Db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AppFileExists(appFile.Id))
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
            return View(appFile);
        }

        // GET: AppFiles/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var appFile = await Db.AppFile
                .FirstOrDefaultAsync(m => m.Id == id);
            if (appFile == null)
            {
                return NotFound();
            }

            return View(appFile);
        }

        // POST: AppFiles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var appFile = await Db.AppFile.FindAsync(id);
            Db.AppFile.Remove(appFile);
            await Db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AppFileExists(Guid id)
        {
            return Db.AppFile.Any(e => e.Id == id);
        }
    }
}
