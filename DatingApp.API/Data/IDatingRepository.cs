using System.Collections.Generic;
using System.Threading.Tasks;
using DatingApp.API.DTOs;
using DatingApp.API.Helpers;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IDatingRepository
    {
         void Add<T>(T entity) where T: class;
         void Delete<T>(T entity) where T: class;
         Task<bool> SaveAll();
         Task<PagedList<User>> GetUsers(UserParams userParams);
         Task<User> GetUser(int Id, bool isCurrentUser);
         Task<Photo> GetPhoto(int Id); 
         Task<Photo> GetMainPhotoForUser(int userId);
         Task<Like> GetLike(int userId, int receipientId);
         Task<Message> GetMessage(int id);
         Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams);
         Task<IEnumerable<Message>> GetMessageThread(int userId, int receipientId);
         Task<List<PhotoForApprovalDto>> GetUnApprovedPhotos();
         Task<Photo> GetNotApprovedPhoto(int photoId);
         Task<Photo> RejectPhotos(int photoId);

    }
}