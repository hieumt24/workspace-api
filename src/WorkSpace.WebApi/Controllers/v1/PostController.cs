using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.Extensions;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.WebApi.Controllers.v1
{
    [ApiController]
    [Route("api/v1/posts")]
    public class PostController : ControllerBase
    {
        private readonly IPostRepository _postRepository;

        public PostController(IPostRepository postRepository)
        {
            _postRepository = postRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var posts = await _postRepository.GetAllPostsAsync();
            return Ok(posts);
        }

        [HttpGet("featured")]
        public async Task<IActionResult> GetAllFeaturedPosts()
        {
            var posts = await _postRepository.GetAllFeaturedPostsAsync();
            return Ok(posts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _postRepository.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] WorkSpace.Domain.Entities.Post post)
        {
            
            int userId = User.GetUserId();
            await _postRepository.AddPostAsync(userId, post);
            return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] WorkSpace.Domain.Entities.Post post)
        {
            if (id != post.Id)
            {
                return BadRequest();
            }
          
            int userId = User.GetUserId();
            await _postRepository.UpdatePostAsync(userId, post);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _postRepository.DeletePostAsync(id);
            return NoContent();
        }


    }
}
