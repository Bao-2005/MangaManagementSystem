using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaManagementSystem.Business.DTOs.Requests;
using MangaManagementSystem.Business.DTOs.Responses;

namespace MangaManagementSystem.Business.Services.Interfaces;

public interface IPageTaskService
{
    Task<PageTaskResponse> CreateAsync(Guid mangakaId, CreatePageTaskRequest request);
    Task<IEnumerable<PageTaskResponse>> GetMangakaTasksAsync(Guid mangakaId);
    Task<IEnumerable<PageTaskResponse>> GetAssistantTasksAsync(Guid assistantId);
    Task<PageTaskResponse> SubmitAsync(Guid assistantId, Guid pageTaskId, SubmitPageTaskRequest request);
    Task<PageTaskResponse> ApproveSubmissionAsync(Guid mangakaId, Guid submissionId);
    Task<PageTaskResponse> RejectSubmissionAsync(Guid mangakaId, Guid submissionId, ReviewPageTaskSubmissionRequest request);
}

