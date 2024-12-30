using Minio;
using Minio.Exceptions;
using Minio.DataModel.Args;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class MinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _bucketName;

    // Constructeur MinioService
    public MinioService(IConfiguration configuration)
    {
        // Récupération des paramètres de configuration pour Minio
        var endpoint = configuration["Minio:Endpoint"];
        var accessKey = configuration["Minio:AccessKey"];
        var secretKey = configuration["Minio:SecretKey"];
        _bucketName = configuration["Minio:BucketName"];

        // Initialisation du client Minio avec les paramètres de configuration
        _minioClient = new MinioClient()
            .WithEndpoint(endpoint)
            .WithCredentials(accessKey, secretKey)
            .Build();
    }

    // Méthode pour uploader un fichier sur Minio
    public async Task UploadFileAsync(string objectName, string content)
    {
        try
        {
            // Vérifie si le bucket existe
            var found = await _minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(_bucketName));
            if (!found)
            {
                // Si le bucket n'existe pas, le créer
                await _minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(_bucketName));
            }

            // Convertir le contenu en stream et uploader l'objet sur Minio
            using (var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content)))
            {
                await _minioClient.PutObjectAsync(new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType("application/json"));
            }
        }
        catch (MinioException e)
        {
            // Gérer les exceptions Minio
            Console.WriteLine($"[Minio] Error: {e}");
        }
    }

    // Méthode pour télécharger un fichier depuis Minio
    public async Task<Stream> GetFileAsync(string objectName)
    {
        try
        {
            var stream = new MemoryStream();
            // Télécharger l'objet depuis Minio et copier le contenu dans un stream
            await _minioClient.GetObjectAsync(new GetObjectArgs()
                .WithBucket(_bucketName)
                .WithObject(objectName)
                .WithCallbackStream((s) => s.CopyTo(stream)));
            stream.Position = 0; // Réinitialiser la position du stream
            return stream;
        }
        catch (MinioException e)
        {
            // Gérer les exceptions Minio
            Console.WriteLine($"[Minio] Error: {e}");
            return null;
        }
    }
}