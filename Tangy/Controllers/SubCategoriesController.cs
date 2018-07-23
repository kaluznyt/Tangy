using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tangy.Data;
using Tangy.Models;
using Tangy.Models.ViewModels;
using Tangy.Utility;

namespace Tangy.Controllers
{
    [Authorize(Roles = StaticDetails.AdminEndUser)]
    public class SubCategoriesController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        [TempData]
        public string StatusMessage { get; set; }

        public SubCategoriesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        // GET
        public async Task<IActionResult> Index()
        {
            var subCategories = _dbContext.SubCategory.Include(s => s.Category);

            return View(await subCategories.ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            var viewModel = await FetchSubCategoryAndCategoryViewModel();

            return View(viewModel);
        }

        private async Task<SubCategoryAndCategoryViewModel> FetchSubCategoryAndCategoryViewModel()
        {
            var viewModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await _dbContext.Category.ToListAsync(),
                SubCategory = new SubCategory(),
                SubCategoryList = await _dbContext.SubCategory.OrderBy(p => p.Name).Select(p => p.Name).Distinct().ToListAsync()
            };
            return viewModel;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid && ValidateSubCategoryViewModelOnCreate(model))
            {
                _dbContext.Add(model.SubCategory);
                await _dbContext.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }


            var viewModel = await FetchSubCategoryAndCategoryViewModel();
            viewModel.SubCategory = model.SubCategory;
            viewModel.StatusMessage = StatusMessage;

            return View(viewModel);

        }

        private bool ValidateSubCategoryViewModelOnCreate(SubCategoryAndCategoryViewModel model)
        {
            var doesSubCategoryExist = _dbContext.SubCategory
                .Any(p => p.Name == model.SubCategory.Name);

            var doesSubCategoryCategoryCombinationExist = _dbContext.SubCategory
                .Any(p => p.CategoryId == model.SubCategory.CategoryId 
                          && p.Name == model.SubCategory.Name);

            if (doesSubCategoryExist && model.IsNew)
            {
                StatusMessage = "Error: Sub Category Name already exists";
                return false;
            }

            if (!doesSubCategoryExist && !model.IsNew)
            {
                StatusMessage = "Error: Sub Category does not exist";
                return false;
            }

            if (doesSubCategoryCategoryCombinationExist)
            {
                StatusMessage = "Error: Category and Sub Category both exist";
                return false;
            }

            return true;
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var subCategory = await _dbContext.SubCategory.FindAsync(id);

            if (subCategory == null)
            {
                return NotFound();
            }

            var model = await FetchSubCategoryAndCategoryViewModel();
            model.SubCategory = subCategory;

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid && ValidateSubCategoryViewModelOnEdit(model))
            {
                var subCategory = await _dbContext.SubCategory.FindAsync(id);

                subCategory.Name = model.SubCategory.Name;
                subCategory.CategoryId = model.SubCategory.CategoryId;
                await _dbContext.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var viewModel = await FetchSubCategoryAndCategoryViewModel();
            viewModel.StatusMessage = StatusMessage;
            viewModel.SubCategory = model.SubCategory;

            return View(viewModel);
        }

        private bool ValidateSubCategoryViewModelOnEdit(SubCategoryAndCategoryViewModel model)
        {
            var doesSubCategoryExist = _dbContext.SubCategory.Any(p => p.Name == model.SubCategory.Name);
            var doesCategoryExist = _dbContext.SubCategory.Any(p => p.CategoryId == model.SubCategory.CategoryId);

            if (!doesSubCategoryExist)
            {
                StatusMessage = "Error: Sub Category doesnt' exist, cannot edit non-existing category";
                return false;
            }

            if (doesCategoryExist)
            {
                StatusMessage = "Error: Category and Sub Category combination already exists";
                return false;
            }

            return true;
        }


        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            return await DetailsDeleteCommonAction(id);
        }


        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            return await DetailsDeleteCommonAction(id);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var subCategory = await _dbContext.SubCategory.SingleOrDefaultAsync(m => m.Id == id);
            _dbContext.SubCategory.Remove(subCategory);
            await _dbContext.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }



        private async Task<IActionResult> DetailsDeleteCommonAction(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }

            var subCategory =
                await _dbContext.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }
    }
}