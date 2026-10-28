using project_photo_sharing.Models;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace project_photo_sharing.Controllers
{
    public class HomeController : Controller
    {
        private PhotoSharingDbContext db = new PhotoSharingDbContext();
        private int CurrentUserId = 1; // Giả lập user đang đăng nhập (ban_cua_toi)

        public ActionResult Index()
        {
            var posts = db.PhotoPosts
                          .Include(p => p.Author)
                          .Include(p => p.Comments.Select(c => c.User))
                          .OrderByDescending(p => p.CreatedAt)
                          .ToList();

            var followedIds = db.Follows
                                .Where(f => f.FollowerId == CurrentUserId)
                                .Select(f => f.FollowingId)
                                .ToList();

            foreach (var post in posts)
            {
                post.IsFollowedByCurrentUser = followedIds.Contains(post.UserId);
            }

            return View(posts);
        }

        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase imageFile, string caption)
        {
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string uploadsFolder = Server.MapPath("~/Uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                imageFile.SaveAs(filePath);

                var newPost = new PhotoPost
                {
                    UserId = CurrentUserId,
                    ImageUrl = "/Uploads/" + uniqueFileName,
                    Caption = caption ?? "",
                    CreatedAt = DateTime.Now
                };

                db.PhotoPosts.Add(newPost);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult EditPost(int postId, string newCaption, HttpPostedFileBase newImageFile)
        {
            var post = db.PhotoPosts.Find(postId);
            if (post == null || post.UserId != CurrentUserId)
            {
                return new HttpStatusCodeResult(403, "Bạn không có quyền sửa bài viết này!");
            }

            if (!string.IsNullOrWhiteSpace(newCaption))
            {
                post.Caption = newCaption.Trim();
            }

            if (newImageFile != null && newImageFile.ContentLength > 0)
            {
                if (post.ImageUrl.StartsWith("/Uploads/"))
                {
                    string oldPath = Server.MapPath("~" + post.ImageUrl);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(newImageFile.FileName);
                string newPath = Path.Combine(Server.MapPath("~/Uploads"), uniqueFileName);
                newImageFile.SaveAs(newPath);

                post.ImageUrl = "/Uploads/" + uniqueFileName;
            }

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult DeletePost(int postId)
        {
            var post = db.PhotoPosts.Include(p => p.Comments).FirstOrDefault(p => p.PostId == postId);
            if (post == null || post.UserId != CurrentUserId)
            {
                return new HttpStatusCodeResult(403, "Bạn không có quyền xóa!");
            }

            db.Comments.RemoveRange(post.Comments);

            if (post.ImageUrl.StartsWith("/Uploads/"))
            {
                string physicalPath = Server.MapPath("~" + post.ImageUrl);
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }
            }

            db.PhotoPosts.Remove(post);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult AddComment(int postId, string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var cmt = new Comment
                {
                    PostId = postId,
                    UserId = CurrentUserId,
                    Content = content.Trim(),
                    CreatedAt = DateTime.Now
                };
                db.Comments.Add(cmt);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ToggleFollow(int targetUserId)
        {
            if (targetUserId != CurrentUserId)
            {
                var existingFollow = db.Follows.FirstOrDefault(f => f.FollowerId == CurrentUserId && f.FollowingId == targetUserId);
                if (existingFollow != null)
                {
                    db.Follows.Remove(existingFollow);
                }
                else
                {
                    db.Follows.Add(new Follow
                    {
                        FollowerId = CurrentUserId,
                        FollowingId = targetUserId,
                        CreatedAt = DateTime.Now
                    });
                }
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}