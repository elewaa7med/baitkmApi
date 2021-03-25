//using Swashbuckle.AspNetCore.Swagger;
//using Swashbuckle.AspNetCore.SwaggerGen;

//namespace Baitkm.SwaggerHelpers
//{
//    public class SwaggerHelper : IOperationFilter
//    {
//        public void Apply(Operation operation, OperationFilterContext context)
//        {
//            if (operation.OperationId.ToLower() != "tokenpost") return;
//            operation.Parameters.Clear();
//            operation.Parameters.Add(new NonBodyParameter
//            {
//                Name = "login",
//                In = "formData",
//                Required = true,
//                Type = "string"
//            });
//            operation.Parameters.Add(new NonBodyParameter
//            {
//                Name = "password",
//                In = "formData",
//                Required = true,
//                Type = "string",
//            });
//            operation.Consumes.Add("multipart/form-data");
//        }
//    }
//}
