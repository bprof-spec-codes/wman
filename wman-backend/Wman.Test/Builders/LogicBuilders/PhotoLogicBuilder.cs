﻿using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wman.Data.DB_Models;
using Wman.Logic.Services;
using Wman.Repository.Interfaces;

namespace Wman.Test.Builders.LogicBuilders
{
    class PhotoLogicBuilder
    {
        public static Mock<IPicturesRepo> GetPicturesRepo(List<Pictures> pictureList)
        {
            throw new NotImplementedException();
        }

        public static List<Pictures> GetPictures()
        {
            List<Pictures> pictureList = new();

            pictureList.Add(new Pictures
            {
                Id = 0,
                Name = "My Cat",
                Url = "https://cdn.discordapp.com/attachments/432444267802132480/903766815962988604/catto.PNG",
                PicturesType = PicturesType.ProfilePic
            });

            pictureList.Add(new Pictures
            {
                Id = 1,
                Name = "Cats be like",
                Url = "https://cdn.discordapp.com/attachments/319425499136917504/888156347068219422/242095608_1758714227659063_7552462664503029975_n.png",
                PicturesType = PicturesType.ProfilePic
            });

            pictureList.Add(new Pictures
            {
                Id = 2,
                Name = "Doggos",
                Url = "https://cdn.discordapp.com/attachments/381520882608373761/885076845232521217/Tastes-like-chicken-545x731.png",
                PicturesType = PicturesType.ProofOfWorkPic
            });

            pictureList.Add(new Pictures
            {
                Id = 3,
                Name = "Cat steal",
                Url = "https://cdn.discordapp.com/attachments/319425499136917504/882311734872932352/241138468_4317513471699981_2564007576925827274_n.png",
                PicturesType = PicturesType.ProofOfWorkPic
            });

            pictureList.Add(new Pictures
            {
                Id = 4,
                Name = "Found a spider at work(dont open)",
                Url = "https://cdn.discordapp.com/attachments/417401074660540427/911767393104175114/RDT_20211119_2343021227032254472691592.jpg",
                PicturesType = PicturesType.ProofOfWorkPic
            });

            return pictureList;
        }

        public static Mock<IPhotoService> GetPhotoService()
        {
            var mock = new Mock<IPhotoService>();

            mock.Setup(x => x.AddProfilePhotoAsync(It.IsAny<IFormFile>())).Returns(taskImageUploadHelper);
            mock.Setup(x => x.DeleteProfilePhotoAsync(It.IsAny<string>())).Returns(taskDeletionHelper);

            return mock;
        }

        private static Task<ImageUploadResult> taskImageUploadHelper()
        {
            var akarmi = new ImageUploadResult();
            return Task.FromResult(akarmi);
        }

        private static Task<DeletionResult> taskDeletionHelper()
        {
            var akarmi = new DeletionResult();
            return Task.FromResult(akarmi);
        }
    }
}
