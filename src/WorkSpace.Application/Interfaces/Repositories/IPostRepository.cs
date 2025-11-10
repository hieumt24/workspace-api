using System.Collections.Generic;
using System.Threading.Tasks;
using WorkSpace.Domain.Entities;
using WorkSpace.Application.DTOs.Post;

namespace WorkSpace.Application.Interfaces.Repositories
{
    public interface IPostRepository
    {
        Task<List<Post>> GetAllPostsAsync();
        Task<List<PostDto>> GetAllFeaturedPostsAsync();
        Task<Post?> GetPostByIdAsync(int id);
        Task AddPostAsync(int userId, Post post);
        Task UpdatePostAsync(int userId, Post post);
        Task DeletePostAsync(int id);
    }
}
