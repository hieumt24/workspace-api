using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Post;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly WorkSpaceContext _context;

        public PostRepository(WorkSpaceContext dbContext)
        {
            _context = dbContext;
        }

     
        public async Task<List<Post>> GetAllPostsAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreateUtc)
                .ToListAsync();
        }

    
        public async Task<List<PostDto>> GetAllFeaturedPostsAsync()
        {
            return await _context.Posts
                .Where(p => p.IsFeatured)
                .Select(p => new PostDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    ContentMarkdown = p.ContentMarkdown,
                    ContentHtml = p.ContentHtml,
                    ImageData = p.ImageData,
                    IsFeatured = p.IsFeatured,
                    CreateUtc = p.CreateUtc,
                    UserName = p.User != null ? p.User.UserName : "Ẩn danh",
                    Avatar = p.User != null ? p.User.Avatar : null
                })
                .OrderByDescending(p => p.CreateUtc)
                .ToListAsync();
        }

     
        public async Task<Post?> GetPostByIdAsync(int id)
        {
            return await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

     
        public async Task AddPostAsync(int userId, Post post)
        {
            post.UserId = userId;
            post.CreateUtc = DateTime.UtcNow;
            post.LastModifiedUtc = DateTime.UtcNow;

            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
        }

   
        public async Task UpdatePostAsync(int userId, Post post)
        {
            var existingPost = await _context.Posts.FindAsync(post.Id);
            if (existingPost == null)
                throw new Exception("Post not found.");

            existingPost.Title = post.Title;
            existingPost.ContentMarkdown = post.ContentMarkdown;
            existingPost.ContentHtml = post.ContentHtml;
            existingPost.ImageData = post.ImageData;
            existingPost.IsFeatured = post.IsFeatured;
            existingPost.LastModifiedUtc = DateTime.UtcNow;
            existingPost.UserId = userId;

            _context.Posts.Update(existingPost);
            await _context.SaveChangesAsync();
        }


        public async Task DeletePostAsync(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            if (post == null)
                throw new Exception("Post not found.");

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
        }
    }
}
