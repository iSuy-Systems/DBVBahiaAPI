using DBVBahia.Business.Intefaces;
using DBVBahia.Business.Models;
using DBVBahia.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DBVBahia.Data.Repository
{
    public class PictureRepository : Repository<Picture>, IPictureRepository
	{
        public PictureRepository(DBVBahiaDbContext context) : base(context)
        {

        }

    }
}