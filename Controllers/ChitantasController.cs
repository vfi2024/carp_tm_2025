using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using carp_tm_2025.Data;
using carp_tm_2025.Models;
using System.Drawing;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using iText.Kernel.Geom;
using X.PagedList;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Org.BouncyCastle.Crypto;
using X.Web.PagedList;
using X.PagedList.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Org.BouncyCastle.Utilities;
using System.Net;
using System.Runtime.Versioning;
//


namespace carp_tm_2025.Controllers
{
    [Authorize(Roles = "CARP_TM\\CARP_TM_READ")]
    public class ChitantasController : Controller
    {
        private readonly carp_tm_2025Context _context;

        public ChitantasController(carp_tm_2025Context context)
        {
            _context = context;
        }

        // GET: Chitantas
        public async Task<IActionResult> Index(int nrcarnet = 99999)
        {
            var chitante_nrcarnet = await _context.Chitantas
                            .Where(cn => cn.nrcarnet == nrcarnet).OrderByDescending(cn => cn.data).ToListAsync();

            ViewData["nr_carnet_ch"] = nrcarnet;

            return View(chitante_nrcarnet);
        }
                


        public string calcul_luna_ajdeces(int nrcarnetGetLunaD, decimal plataAjDecGet)

        {
            DateTime lunaajdeces;
            decimal plata = 0;

           
            decimal rest, dbnda;
            int rest1, rest2 = 0;
            string zero;
            decimal var15000, var20000, var25000, totalold, totalintre = 0;
            DateTime luna10si2001, luna10si2002, lunalast, lunaajdecescalculata = Convert.ToDateTime("01/12/1997");
            var setariajdeces = (from ajdec in _context.ajutordeces select ajdec).Single();
            var15000 = setariajdeces.contributieanterioara;
            var20000 = setariajdeces.contributieveche;
            var25000 = setariajdeces.contributienoua;
            luna10si2001 = setariajdeces.lunaajdecesveche;
            luna10si2002 = setariajdeces.lunaajdecesnoua;
                        

            var pensGelLunaD = _context.Pensionars
                .Where(pgld => pgld.nrcarnet == nrcarnetGetLunaD).SingleOrDefault();


            if (plataAjDecGet < 0) return pensGelLunaD.lunaajdeces.ToString("yyyy/MM/dd");


            lunalast = pensGelLunaD.lunaajdeces;
            //aug 2023 tablet mvc6            
            totalold = (-(lunalast.Year * 12 + lunalast.Month) + (luna10si2001.Year * 12 + luna10si2001.Month) - 1) * var15000;
            totalintre = ((luna10si2002.Year * 12 + luna10si2002.Month) - (luna10si2001.Year * 12 + luna10si2001.Month - 1) - 1) * var20000;
            plata = plataAjDecGet;


            if (lunalast < luna10si2001)
            {
                if (plata <= totalold)
                {
                    var rest15000plata = (plata / var15000) - (Math.Floor(plata / var15000));
                    if (rest15000plata != 0) return "";
                    if (rest15000plata == 0)
                    {
                        int nrluni = Convert.ToInt32(plata / var15000);
                        lunalast.AddMonths(nrluni);
                        //de testat if schimba anul
                        lunalast = lunalast.AddMonths(nrluni);
                        return lunalast.Month.ToString() + "/" + "1" + "/" + lunalast.Year.ToString();
                    }
                }

                if (plata > totalold)
                {
                    if (plata <= (totalintre + totalold))
                    {
                        var rest20000plata = ((plata - totalold) / var20000) - (Math.Floor((plata - totalold) / var20000));
                        if (rest20000plata != 0) return "";
                        if (rest20000plata == 0)
                        {
                            var nrluni = Convert.ToInt32((plata - totalold) / var20000) + lunalast.Month;
                            lunalast = lunalast.AddMonths(nrluni);
                            return lunalast.Month.ToString() + "/" + "1" + "/" + lunalast.Year.ToString();
                        }
                    }

                if (plata >= (totalintre + totalold))
                    {
                        var rest25000plata = ((plata - (totalold + totalintre)) / var25000) - (Math.Floor((plata - (totalold + totalintre)) / var25000));
                        if (rest25000plata != 0) return "";
                        if (rest25000plata == 0)
                        {
                            var nrluni = Convert.ToInt32((plata - (totalold + totalintre)) / var25000) + lunalast.Month;
                            lunalast = lunalast.AddMonths(nrluni);
                            return lunalast.Month.ToString() + "/" + "1" + "/" + lunalast.Year.ToString();
                        }
                    }
                }
            }



            if (lunalast >= luna10si2001)
            {
                totalold = 0;

                if (lunalast < luna10si2002)
                {

                    totalintre = ((luna10si2002.Year * 12 + luna10si2002.Month) - (lunalast.Year * 12 + lunalast.Month) - 1) * var20000;

                    if (plata <= (totalintre + totalold))
                    {

                        var rest20000plata = ((plata - totalold) / var20000) - (Math.Floor((plata - totalold) / var20000));
                        if (rest20000plata != 0) return "";
                        if (rest20000plata == 0)
                        {
                            var nrluni = Convert.ToInt32((plata - totalold) / var20000);
                            lunalast = lunalast.AddMonths(nrluni);
                            return lunalast.Month.ToString() + "/" + "1"  + "/" +  lunalast.Year.ToString();
                            
                        }
                    }

                    if (plata > (totalintre + totalold))
                    {
                        var rest25000plata = ((plata - (totalold + totalintre)) / var25000) - (Math.Floor((plata - (totalold + totalintre)) / var25000));
                        if (rest25000plata != 0) return "";
                        if (rest25000plata == 0)
                        {
                            var nrluni = Convert.ToInt32((plata - (totalold + totalintre)) / var25000);
                            lunalast = lunalast.AddMonths(nrluni);
                            return lunalast.Month.ToString() + "/" + "1" + "/" + lunalast.Year.ToString();
                        }
                    }
                }
            }


            if (lunalast >= luna10si2002)
            {
                totalold = 0;
                totalintre = 0;

                var rest25000plata = ((plata - (totalold + totalintre)) / var25000) - (Math.Floor((plata - (totalold + totalintre)) / var25000));
                if (rest25000plata != 0) return "";
                if (rest25000plata == 0)
                {
                    var nrluni = Convert.ToInt32((plata - (totalold + totalintre)) / var25000);
                    lunalast = lunalast.AddMonths(nrluni);
                    return lunalast.Month.ToString() + "/" + "1" + "/" + lunalast.Year.ToString();
                }
            }


          


            return "";

        }


        // GET: Chitantas/Create
        public IActionResult Create(int idc, string este_NC = "0")
        {
            ViewData["este_NC"] = este_NC; 

            //de aici get net 8

            var pensionarnrcarnetd = (from p in _context.Pensionars
                                      where (p.nrcarnet == idc)
                                      select p).Single();


            if (pensionarnrcarnetd.id_stare == 30)
            {

                ViewData["Error"] = "Persoana este decedata";
                return View("Error");
            }



            ViewData["nume"] = pensionarnrcarnetd.nume;

            //GET NR CH
            string userId = User.Identity.Name;                    
            
            var getnrch = _context.nr_chitanta
                           .Where(p => p.nume_user == userId).FirstOrDefault();
            ViewData["nr_chitanta"] = getnrch.nrchitanta + 1;
            ViewData["id_utilizator"] = getnrch.id_user;


            if ((getnrch.id_user >10) && (este_NC== "NC"))
            {

                ViewData["Error"] = "Nu se pot introduce note contabile de catre delegati";
                return View("Error");
            }

            //RESTRICTII
            //NU DECEDAT SAU EXCLUS
            //? MAI TREBUIE? nu IF DESFASURATOR<>0
            var testexcl = _context.Pensionars
               .Where(p => p.nrcarnet == idc && ((p.id_stare == 1) || (p.id_stare == 51) || (p.id_stare == 52)));

            if (testexcl.Count() > 0)
            {

                ViewData["Error"] = "Persoana este decedata, exclusa sau a depus cerere de retragere";
                return View("Error");
            }


            if (pensionarnrcarnetd.soldimp > 0)
                if (pensionarnrcarnetd.desfasurator != 0)
                {
                    ViewData["Error"] = "Nu a fost creat desfasurator - exista restante la plata integrala a ratelor ";
                    return View("Error");
                }

            //SHOW GET NRCARNET, DATA
            ViewData["partidach"] = idc;
            ViewData["data"] = DateTime.Now;

            DateTime now = DateTime.Now;
            ViewData["dobanda"] = calcul_dobanda_desfasurator(idc, DateTime.Now);
            ViewData["dobanda_penalizatoare"] = calcul_dobanda_penalizatoare(idc, DateTime.Now);

           // if (pensionarnrcarnetd.nrcarnet == 46141665) ViewData["dobanda"] = 98.64M;
            


            if (pensionarnrcarnetd.soldimp > 0)
            {
                var rata_now = _context.desfasurator_rate
                  .Where(dd => (dd.nrcarnet == idc) && (dd.data_rata.Month == DateTime.Now.Month) && (dd.data_rata.Year == DateTime.Now.Year)).SingleOrDefault();

                ViewData["rata_de_plata"] = rata_now.rata_de_plata;
                ViewData["id_impr_desf_ch"] = rata_now.id_imprumut;

            }
            if (pensionarnrcarnetd.soldimp <= 0) ViewBag.rata_de_plata = "0";

            @ViewData["nrcarnet_ad"] = idc;
            @ViewData["sold_impr_ad"] = pensionarnrcarnetd.soldimp;
            @ViewData["nrcarnet_ld"] = idc;

            //end get net 8

            

            return View();
        }

        // POST: Chitantas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("idchitanta,nrch,data,nrcarnet,cotizatie,ajdeces,lunaajdeces,rata_de_plata,rata,diferenta_rata,dobanda_penalizatoare,dobanda,credit_imprumut,debit_imprumut,taxainscr,total,nume,id_utilizator,analitic,tip_operatie,tip_document,serie")] Chitanta chitanta)
        {

          //  var chgetnr=await _context.Chitantas.Where(cgn => (cgn.nrch == chitanta.nrch) && (cgn.data.Month == chitanta.data.Month)
          //  && (cgn.data.Year == chitanta.data.Year) && (cgn.data.Day==chitanta.data.Day) && (chitanta.tip_document == 0) && (chitanta.nrch!=1)).ToListAsync();
        ////    if (chgetnr.Count()>0)
          //  {
           //     ViewData["Error"] = "Aceasta chitanta a mai fost introdusa";
           //     return View("Error");
          //  }

            if ((chitanta.tip_document == 6) && (chitanta.rata != 0))
            {
                ViewData["Error"] = "Nu se poate face stornare pentru rata sau dobanda";
                return View("Error");
            }




            //IUNIE 2024 TABLETS 

            //5 RESTRICTII
            //nu este data  curenta   
            //exista CH cu data mai amre ca data introdusa
            //nu da corelatia
            //credit si rata mai mare ca sold si debit            
            //rata negativa 

            DateTime now2 = DateTime.Now;
            decimal dif_rata_creare_ch = 0;
            decimal rata_ch_ramasa = 0;
            int tip_document_rj_rc = 0;
            int test_nuRJnuUseC = 0;
            int test_dataCHIns = 0;
            int test_nuDesf = 0;
            int test_db = 0;

            test_dataCHIns = 0;
            if ((chitanta.data.Year) != (DateTime.Now.Year)) test_dataCHIns = 1;
            if ((chitanta.data.Month) != (DateTime.Now.Month)) test_dataCHIns = 1;
            if ((chitanta.data.Month) == (DateTime.Now.Month))
                if ((chitanta.data.Day) != (DateTime.Now.Day))
                    test_dataCHIns = 1;
            if (test_dataCHIns == 1)
            {
                ViewData["Error"] = "Nu se pot introduce chitante decat pentru data curenta";
                return View("Error");
            }


            if ((chitanta.cotizatie + chitanta.ajdeces + chitanta.rata + chitanta.dobanda + chitanta.taxainscr + chitanta.dobanda_penalizatoare) != chitanta.total)
            {
                ViewData["Error"] = "Nu da corelatia la chitanta";
                return View("Error");
            }


            var testsold = await _context.Pensionars
                 .Where(t => t.nrcarnet == chitanta.nrcarnet && t.soldimp > 0).ToListAsync();
            if (testsold.Count() > 0)
                if ((testsold.First().credit_imprumut + chitanta.rata) > (testsold.First().soldimp + testsold.First().debit_imprumut))
                {
                    ViewData["Error"] = "credit si rata  mai mare ca sold imprumut";
                    return View("Error");
                }


            if ((chitanta.ajdeces >= 0) && (chitanta.cotizatie >= 0))
            {
                if (testsold.Count() > 0 && chitanta.rata <= 0) 
                {
                    ViewData["Error"] = "Trebuie sa platiti rata";
                    return View("Error");
                }
            }


            if (chitanta.rata < 0)
            {
                ViewData["Error"] = "Rata negativa";
                return View("Error");

            }

            string userId = User.Identity.Name;

            var pens_get_stare = await _context.Pensionars.Where(pgs => pgs.nrcarnet == chitanta.nrcarnet).SingleOrDefaultAsync();
            test_nuDesf = 0;
            if (pens_get_stare.id_stare == 3) test_nuDesf = 1;
            if (pens_get_stare.id_stare == 4) test_nuDesf = 1;


            if (pens_get_stare.id_stare == 30)
            {

                ViewBag.msgerr = "Persoana este decedata";
                return View("Error");
            }


            var update_desf = await _context.desfasurator_rate
                   .Where(ud => ((ud.nrcarnet == chitanta.nrcarnet) && (ud.data_rata.Month == chitanta.data.Month) && (ud.data_rata.Year == chitanta.data.Year))).SingleOrDefaultAsync();
            if (test_nuDesf == 0)
                if ((chitanta.rata > 0) && (update_desf == null))
                {
                    ViewData["Error"] = "Nu exista in desfasurator rata cu aceasta data";
                    return View("Error");
                }

            int idcad;
            idcad = 0;
            if ((pens_get_stare.debit_imprumut>0) && (pens_get_stare.lunaimp.Month==DateTime.Now.Month) && (pens_get_stare.lunaimp.Year==DateTime.Now.Year)
                     && (pens_get_stare.id_stare==0))
            {
                idcad = Int32.Parse("4612" + pens_get_stare.nrcarnet.ToString());
                var rj2debit = await _context.registru_jurnal.Where(rj2d => (rj2d.id_CA_debitor == idcad) &&  (rj2d.data.Month == DateTime.Now.Month) &&
                                   (rj2d.data.Year == DateTime.Now.Year)).SingleOrDefaultAsync();
                if (rj2debit!=null)
                {
                  pens_get_stare.debit_imprumut = pens_get_stare.debit_imprumut - rj2debit.credit;
                  rj2debit.credit = 0;
                    rj2debit.debit = 0;
                  await  _context.SaveChangesAsync();
                }

            }
            //end before is valid

            if (ModelState.IsValid)

            {

                //start valid
                if (chitanta.total < 0) chitanta.tip_operatie = "plata";

                if (chitanta.total >= 0) chitanta.tip_operatie = "incasare";

                if (chitanta.tip_document != 0)  chitanta.tip_operatie = "NC";

            

                if (pens_get_stare.id_stare == 4) chitanta.tip_document = 4;

                test_db = 0;
                if ((chitanta.tip_document == 1) && (pens_get_stare.id_stare == 3))
                {
                    chitanta.tip_document = 3;
                    test_db = 1;
                }
                if ((test_db == 0) && (pens_get_stare.id_stare == 3))
                    chitanta.tip_document = 2;
                //end valid until add ch

                if (chitanta.id_utilizator > 10) chitanta.tip_document = 7;


                //de aici TRANZACTIE

                using (var transaction = await _context.Database.BeginTransactionAsync())
                {


                    var cek_dbl_ch = await _context.Chitantas.Where(cdc => (cdc.nrch == chitanta.nrch) && (cdc.tip_operatie == "incasare") && (cdc.total>0)).ToListAsync();
                    
                    if (cek_dbl_ch.Count > 0)
                    {
                       ViewData["Error"] = "Mai exista acest numar de chitanta";
                       if ((chitanta.total>0) && (chitanta.nrch!=5) && (chitanta.nrch != 4)) return View("Error");
                       // if (chitanta.nrch != 3) return View("Error");
                    }



                    try
                    {
                     _context.Add(chitanta);
                     await _context.SaveChangesAsync();

                     //after save ch
                     test_nuRJnuUseC = 0;
                     if (pens_get_stare.id_stare == 3) test_nuRJnuUseC = 1;
                     if (pens_get_stare.id_stare == 4) test_nuRJnuUseC = 1;
                     if (chitanta.rata<0) test_nuRJnuUseC = 1;
                     if (chitanta.dobanda < 0) test_nuRJnuUseC = 1;
                     //stare
                     //1 = decedat
                     //2 = exclus
                     //3 = debitor decedat
                     //4= debitor popriri                

                     //id document
                     //0 ch
                     //1 banca
                     //2 debitor d casa
                     //3 debitor D banca
                     //4 debitor popriri
                     //7 delegat

                     update_nrch(chitanta.nrch, userId);
               
                     update_pensionar_chit(chitanta.nrcarnet, chitanta.rata_de_plata, chitanta.rata, chitanta.cotizatie, chitanta.data, chitanta.lunaajdeces, "C", chitanta.ajdeces, chitanta.dobanda, chitanta.taxainscr, chitanta.tip_document, chitanta.nrch);                           
          
                     if (pens_get_stare.id_stare == 3)
                       update_rj_debitori_d(chitanta.idchitanta, chitanta.nrch, chitanta.nrcarnet, chitanta.data, chitanta.rata, chitanta.tip_document, chitanta.id_utilizator);

                     if (pens_get_stare.id_stare == 4)
                       update_rj_debitori_p(chitanta.idchitanta, chitanta.nrch, chitanta.nrcarnet, chitanta.data, chitanta.dobanda, chitanta.dobanda_penalizatoare, chitanta.rata, chitanta.tip_document);

                     if (test_nuRJnuUseC == 0)
                        if (chitanta.rata != 0)
                    {
                        rata_ch_ramasa = utilizare_credit_rata_imprumut(chitanta.nrcarnet, chitanta.idchitanta, chitanta.nrch, chitanta.rata_de_plata, chitanta.rata, chitanta.data, update_desf.rata_platita, chitanta.dobanda, chitanta.tip_document, chitanta.dobanda_penalizatoare);                                            

                        if (rata_ch_ramasa >= chitanta.rata_de_plata)
                        {
                                update_desf.rata_platita = update_desf.rata_de_plata;
                                update_desf.rata_platita_initial = update_desf.rata_de_plata;
                                _context.SaveChanges();
                        }

                        if (rata_ch_ramasa < chitanta.rata_de_plata)
                        {
                                update_desf.rata_platita = rata_ch_ramasa;
                                update_desf.rata_platita_initial = rata_ch_ramasa;
                                _context.SaveChanges();
                        }
                        
                    }

                     //update RJ
                     if (test_nuRJnuUseC == 0)
                      update_registru_jurnal(chitanta.idchitanta, chitanta.nrch, chitanta.nrcarnet, chitanta.data, chitanta.dobanda, chitanta.cotizatie, chitanta.taxainscr, chitanta.ajdeces, chitanta.dobanda_penalizatoare, chitanta.tip_document);
               
              
                     if ((chitanta.rata<0) && (chitanta.tip_document==6))
                     {                    

                    var chNC2rata = await _context.Chitantas.Where(ch2r => (ch2r.nrcarnet == chitanta.nrcarnet) && (ch2r.rata > 0) && (ch2r.data.Month == DateTime.Now.Month) && (ch2r.data.Year==DateTime.Now.Year)).OrderByDescending(ch2r=>ch2r.data).ToListAsync();
                    if (chNC2rata.Count ==0)
                    {

                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata decat pentru luna curenta";
                        return View("Error");
                    }

                    if (chNC2rata.Count >1)
                    {

                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata daca sunt mai multe chitante in luna respectiva ";
                        return View("Error");
                    }

                    var chNC2Credit = await _context.Chitantas.Where(ch2r => (ch2r.nrcarnet == chitanta.nrcarnet) && (ch2r.credit_imprumut > 0) && (ch2r.data.Month == DateTime.Now.Month) && (ch2r.data.Year == DateTime.Now.Year)).SingleOrDefaultAsync();
                    if (chNC2Credit == null)
                    {

                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata daca au fost rambursari anticipate in luna curenta";
                        return View("Error");
                    }


                    var RJNC2Rata = await _context.registru_jurnal.Where(rjnc2r => (rjnc2r.id_document == chNC2rata.First().idchitanta) && (rjnc2r.id_CS_creditor==106)).SingleOrDefaultAsync();
                    if (RJNC2Rata == null)
                    {
                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata daca chitanta anulata nu a avut incasare de credit";
                        return View("Error");
                    }

                    RJNC2Rata = await _context.registru_jurnal.Where(rjnc2r => (rjnc2r.id_document == chNC2rata.First().idchitanta) && (rjnc2r.id_CS_creditor == 107)).SingleOrDefaultAsync();
                    if (RJNC2Rata != null)
                    {
                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata daca chitanta anulata a avut incasare de debit";
                        return View("Error");
                    }

                    int ID_CS_editCHR = 51;
                    int ID_CA_editCHR = 101;
                    string userIdnc2r = User.Identity.Name.Trim();
                    var get_iduser_nc2r_db = _context.utilizatoris
                      .Where(u => u.nume_user == userIdnc2r).SingleOrDefault();
                    int get_iduser_nc2r = get_iduser_nc2r_db.IDUser;
                    if (get_iduser_nc2r == 16) ID_CA_editCHR = 151;
                    if (get_iduser_nc2r == 17) ID_CA_editCHR = 152;
                    if (get_iduser_nc2r == 18) ID_CA_editCHR = 153;
                    if (get_iduser_nc2r == 19) ID_CA_editCHR = 154;
                    if (get_iduser_nc2r == 20) ID_CA_editCHR = 155;
                    if (get_iduser_nc2r == 22) ID_CA_editCHR = 156;
                    if (chitanta.tip_document == 1) ID_CS_editCHR = 52;
                    if (chitanta.tip_document == 1) ID_CA_editCHR = 102;

                    RJNC2Rata = await _context.registru_jurnal.Where(rjnc2r => (rjnc2r.id_document == chNC2rata.First().idchitanta) && (rjnc2r.id_CS_creditor == 106)).SingleOrDefaultAsync();                                
                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = chitanta.data,
                        id_document = chitanta.idchitanta,
                        nr_document = chitanta.nrch.ToString(),
                        id_CS_debitor = ID_CS_editCHR,
                        id_CA_debitor = ID_CA_editCHR,
                        id_CS_creditor = 106,
                        id_CA_creditor = Int32.Parse("4622" + chitanta.nrcarnet.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = 0,
                        SI_CS_credit = 0,
                        SI_CA_credit = pens_get_stare.credit_imprumut,
                        debit = -RJNC2Rata.credit,
                        credit = -RJNC2Rata.credit,
                        SF_CS_debit = 0,
                        SF_CA_debit = 0,
                        SF_CS_credit = 0,
                        SF_CA_credit = pens_get_stare.credit_imprumut - RJNC2Rata.credit,
                        tip_document = chitanta.tip_document,
                        sortare = "H"

                    });
                    pens_get_stare.credit_imprumut = pens_get_stare.credit_imprumut - RJNC2Rata.credit;
                    await _context.SaveChangesAsync();

                    RJNC2Rata = await _context.registru_jurnal.Where(rjnc2r => (rjnc2r.id_document == chNC2rata.First().idchitanta) && (rjnc2r.id_CS_creditor == 103)).SingleOrDefaultAsync();
                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = chitanta.data,
                        id_document = chitanta.idchitanta,
                        nr_document = chitanta.nrch.ToString(),
                        id_CS_debitor = ID_CS_editCHR,
                        id_CA_debitor = ID_CA_editCHR,
                        id_CS_creditor = 103,
                        id_CA_creditor = Int32.Parse("2678" + chitanta.nrcarnet.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = 0,
                        SI_CS_credit = 0,
                        SI_CA_credit = pens_get_stare.soldimp,
                        debit = -RJNC2Rata.credit,
                        credit = -RJNC2Rata.credit,
                        SF_CS_debit = 0,
                        SF_CA_debit = 0,
                        SF_CS_credit = 0,
                        SF_CA_credit = pens_get_stare.soldimp + RJNC2Rata.credit,
                        tip_document = chitanta.tip_document,
                        sortare = "H"

                    });
                    var desfNC2 = await _context.desfasurator_rate.Where(dnc2 => (dnc2.nrcarnet == chitanta.nrcarnet) && (dnc2.data_rata.Month == DateTime.Now.Month) && (dnc2.data_rata.Year == DateTime.Now.Year)).SingleOrDefaultAsync();
                    pens_get_stare.soldimp = pens_get_stare.soldimp + RJNC2Rata.credit;                                     
                    desfNC2.rata_platita = 0;
                    await _context.SaveChangesAsync();

                     }

                     if ((chitanta.dobanda < 0) && (chitanta.tip_document == 6))
                {

                    var chNC2dob = await _context.Chitantas.Where(ch2r => (ch2r.nrcarnet == chitanta.nrcarnet) && (ch2r.rata > 0) && (ch2r.data.Month == DateTime.Now.Month) && (ch2r.data.Year == DateTime.Now.Year)).OrderByDescending(ch2r => ch2r.data).ToListAsync();
                    if (chNC2dob.Count == 0)
                    {

                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata decat pentru luna curenta";
                        return View("Error");
                    }

                    if (chNC2dob.Count > 1)
                    {

                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata daca sunt mai multe chitante in luna respectiva ";
                        return View("Error");
                    }

                    var chNC2DobCredit = await _context.Chitantas.Where(ch2r => (ch2r.nrcarnet == chitanta.nrcarnet) && (ch2r.credit_imprumut > 0) && (ch2r.data.Month == DateTime.Now.Month) && (ch2r.data.Year == DateTime.Now.Year)).SingleOrDefaultAsync();
                    if (chNC2DobCredit == null)
                    {

                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata daca au fost rambursari anticipate in luna curenta";
                        return View("Error");
                    }


                    var RJNC2Dobanda = await _context.registru_jurnal.Where(rjnc2r => (rjnc2r.id_document == chNC2dob.First().idchitanta) && (rjnc2r.id_CS_creditor == 106)).SingleOrDefaultAsync();
                    if (RJNC2Dobanda == null)
                    {
                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata daca chitanta anulata nu a avut incasare de credit";
                        return View("Error");
                    }

                    RJNC2Dobanda = await _context.registru_jurnal.Where(rjnc2r => (rjnc2r.id_document == chNC2dob.First().idchitanta) && (rjnc2r.id_CS_creditor == 107)).SingleOrDefaultAsync();
                    if (RJNC2Dobanda != null)
                    {
                        ViewBag.msgerr = "Nu se poata introduce stornare pentru rata daca chitanta anulata a avut incasare de debit";
                        return View("Error");
                    }

                    int ID_CS_editCHD = 51;
                    int ID_CA_editCHD = 101;
                    string userIdnc2r = User.Identity.Name.Trim();
                    var get_iduser_nc2r_db = _context.utilizatoris
                      .Where(u => u.nume_user == userIdnc2r).SingleOrDefault();
                    int get_iduser_nc2r = get_iduser_nc2r_db.IDUser;
                    if (get_iduser_nc2r == 16) ID_CA_editCHD = 151;
                    if (get_iduser_nc2r == 17) ID_CA_editCHD = 152;
                    if (get_iduser_nc2r == 18) ID_CA_editCHD = 153;
                    if (get_iduser_nc2r == 19) ID_CA_editCHD = 154;
                    if (get_iduser_nc2r == 20) ID_CA_editCHD = 155;
                    if (get_iduser_nc2r == 22) ID_CA_editCHD = 156;
                    if (chitanta.tip_document == 1) ID_CS_editCHD = 52;
                    if (chitanta.tip_document == 1) ID_CA_editCHD = 102;

                    RJNC2Dobanda = await _context.registru_jurnal.Where(rjnc2r => (rjnc2r.id_document == chNC2dob.First().idchitanta) && (rjnc2r.id_CS_creditor == 109)).SingleOrDefaultAsync();
                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = chitanta.data,
                        id_document = chitanta.idchitanta,
                        nr_document = chitanta.nrch.ToString(),
                        id_CS_debitor = ID_CS_editCHD,
                        id_CA_debitor = ID_CA_editCHD,
                        id_CS_creditor = 106,
                        id_CA_creditor = Int32.Parse("4622" + chitanta.nrcarnet.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = 0,
                        SI_CS_credit = 0,
                        SI_CA_credit = pens_get_stare.solddobanda,
                        debit = -RJNC2Dobanda.credit,
                        credit = -RJNC2Dobanda.credit,
                        SF_CS_debit = 0,
                        SF_CA_debit = 0,
                        SF_CS_credit = 0,
                        SF_CA_credit = pens_get_stare.solddobanda + RJNC2Dobanda.credit,
                        tip_document = chitanta.tip_document,
                        sortare = "H"

                    });
                    pens_get_stare.solddobanda = pens_get_stare.solddobanda + RJNC2Dobanda.credit;
                    await _context.SaveChangesAsync();


                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = chitanta.data,
                        id_document = chitanta.idchitanta,
                        nr_document = chitanta.nrch.ToString(),
                        id_CS_debitor = 53,
                        id_CA_debitor = 103,
                        id_CS_creditor = 54,
                        id_CA_creditor = 104,
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = 0,
                        SI_CS_credit = 0,
                        SI_CA_credit = 0,
                        debit = -RJNC2Dobanda.credit,
                        credit = -RJNC2Dobanda.credit,
                        SF_CS_debit = 0,
                        SF_CA_debit = 0,
                        SF_CS_credit = 0,
                        SF_CA_credit = 0,
                        tip_document = chitanta.tip_document,
                        sortare = "H"

                    });


                }

                     await transaction.CommitAsync();

                      return RedirectToAction("Index", new { nrcarnet = chitanta.nrcarnet });

                    }

                catch (Exception ex)
                {
                        // Dacă apare o eroare, facem rollback
                        await transaction.RollbackAsync();


                       // ModelState.AddModelError(string.Empty, "A apărut o eroare la salvare: " + ex.Message);

                        ViewData["Error"] = "Nu s-a putut descarca chitanta " + ex.Message;
                       // return View("Error");
                }

            }

                //END TRANZACTIE

            }
            return View(chitanta);
        }


        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        // GET: Chitantas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            int test_dataCH = 0;
            //NOV 2024 CORE8                   

            if (id == null)
            {
                return NotFound();
            }

            var chitanta = await _context.Chitantas.FindAsync(id);
            if (chitanta == null)
            {
                return NotFound();         


            test_dataCH = 0;
            if ((chitanta.data.Year) != (DateTime.Now.Year)) test_dataCH = 1;
            if ((chitanta.data.Month) != (DateTime.Now.Month)) test_dataCH = 1;
            if ((chitanta.data.Month) == (DateTime.Now.Month))
                if ((chitanta.data.Day) != (DateTime.Now.Day))
                    test_dataCH = 1;
            if (test_dataCH == 1)
            {
                ViewData["Error"] = "Nu se pot face editari decat pentru data curenta";
                return View("Error");
            }      

            }
            return View(chitanta);
        }

        // POST: Chitantas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.


        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("idchitanta,nrch,data,nrcarnet,cotizatie,ajdeces,lunaajdeces,rata_de_plata,rata,diferenta_rata,dobanda_penalizatoare,dobanda,credit_imprumut,debit_imprumut,taxainscr,total,nume,id_utilizator,analitic,tip_operatie,tip_document,serie")] Chitanta chitanta)
        {

            //vineri 22 nov

            if (id != chitanta.idchitanta)
            {
                return NotFound();
            }

            //NOV 2024 CORE8
            int test_dataCHPost = 0;
            int ID_CA_editCH = 0;

            test_dataCHPost = 0;
            if ((chitanta.data.Year) != (DateTime.Now.Year)) test_dataCHPost = 1;
            if ((chitanta.data.Month) != (DateTime.Now.Month)) test_dataCHPost = 1;
            if ((chitanta.data.Month) == (DateTime.Now.Month))
                if ((chitanta.data.Day) != (DateTime.Now.Day))
                    test_dataCHPost = 1;
            if (test_dataCHPost == 1)
            {
                ViewData["Error"] = "Nu se pot face editari decat pentru data curenta";
                return View("Error");
            }


            if ((chitanta.tip_document > 0) && (chitanta.tip_document != 7))
            {
                ViewData["Error"] = "Nu se pot face editari la note contabile";
                //return View("Error");
            }


            if ((chitanta.cotizatie + chitanta.ajdeces + chitanta.rata + chitanta.dobanda + +chitanta.dobanda_penalizatoare + chitanta.taxainscr) != chitanta.total)
            {
                ViewData["Error"] = "Nu da corelatia la chitanta";
                return View("Error");
            }

            int ID_CS_editCH = 51;
            ID_CA_editCH = 101;

            if (chitanta.id_utilizator == 16) ID_CA_editCH = 151;
            if (chitanta.id_utilizator == 17) ID_CA_editCH = 152;
            if (chitanta.id_utilizator == 19) ID_CA_editCH = 154;
            if (chitanta.id_utilizator == 20) ID_CA_editCH = 155;
            if (chitanta.id_utilizator == 18) ID_CA_editCH = 153;
            if (chitanta.id_utilizator == 22) ID_CA_editCH = 156;


            if (chitanta.tip_document == 1) ID_CS_editCH = 52;
            if (chitanta.tip_document == 1) ID_CA_editCH = 102;

            var testsold = _context.Pensionars
                .Where(t => t.nrcarnet == chitanta.nrcarnet && t.soldimp > 0);


            var test_same_lunach = await _context.Chitantas
                       .Where(p => (p.nrcarnet == chitanta.nrcarnet) && (p.data.Month == chitanta.data.Month) && (p.data.Day < chitanta.data.Day) && (p.data.Year == chitanta.data.Year)).ToListAsync();


            string userIdch = User.Identity.Name.Trim();
            var getiduserch = await _context.utilizatoris
                .Where(u => u.nume_user == userIdch).SingleOrDefaultAsync();


            if (ModelState.IsValid)
            {

                var chitanteBeforeEdit = await _context.Chitantas.AsNoTracking()
                       .Where(cbe => cbe.idchitanta == chitanta.idchitanta).SingleOrDefaultAsync();


                if ((chitanteBeforeEdit.cotizatie < 0) && (chitanteBeforeEdit.tip_document != 6))
                {
                    ViewData["Error"] = "Nu se pot face editari pentru excluderi sau decese";
                   // return View("Error");
                }

                if ((chitanteBeforeEdit.cotizatie == 0) && (chitanta.cotizatie < 0))
                {
                    ViewData["Error"] = "Nu se poate face editare decat daca cotizatia a fost anterior actualizarii tot negativa";
                    return View("Error");
                }

                if ((chitanteBeforeEdit.cotizatie <0) && (chitanta.cotizatie == 0))
                {
                    ViewData["Error"] = "Nu se poate face editare decat daca cotizatia va fi dupa actualizare tot negativa";
                    return View("Error");
                }


                if ((chitanteBeforeEdit.ajdeces < 0) && (chitanteBeforeEdit.tip_document != 6))
                {
                    ViewData["Error"] = "Nu se pot face editari pentru excluderi sau decese";
                    // return View("Error");
                }

                if ((chitanta.tip_document == 6))
                {
                    ViewData["Error"] = "Nu se pot face editari pentru stornari";
                    return View("Error");
                }


                var pensEditChPost = await _context.Pensionars
                    .Where(pcep => pcep.nrcarnet == chitanta.nrcarnet).SingleOrDefaultAsync();

                if (chitanteBeforeEdit.tip_document == 6)
                {
                    pensEditChPost.lunaajdeces = chitanta.lunaajdeces;
                    await _context.SaveChangesAsync();
                }


                int idcontanalitic = 0;
                decimal dif_ch = 0;
                int platac, plataajd = 0;
                platac = 0;
                plataajd = 0;
                if (chitanteBeforeEdit.cotizatie < 0) platac = 1;
                if (chitanteBeforeEdit.ajdeces < 0) plataajd = 1;



                //cotiz plata de CEK
                //da  err msg if cotizbeforeedit==0  ...
                //da msg err if cotizbeforeedit!=0 si ch.cotizatie=0

                //only if  cotizbeforeedit!=0 si ch.cotiz!=0
                //de cek if pune corect
                //           dif sold cotizatie pens
                //            dif febit rj
                //?eventiual  test 1,2 cotiz>0 face corect update debit rj si sold cotiz


                //if above face corect si, sf si pt credit si pt debit

                //cotizatie

                if (chitanteBeforeEdit.cotizatie != chitanta.cotizatie)
                {
                    idcontanalitic = Int32.Parse("113" + chitanta.nrcarnet.ToString());

                    var RJAfterEditCotizC = _context.registru_jurnal
                            .Where(rcae => (rcae.data > chitanta.data) && (rcae.id_CA_creditor == idcontanalitic));


                    var RJAfterEditCotizD = _context.registru_jurnal
                            .Where(rcae => (rcae.data > chitanta.data) && (rcae.id_CA_debitor == idcontanalitic));

                    decimal difsoldcotiz = 0;
                    decimal difincasCotiz = 0;
                    decimal difplataCotiz = 0;
                    difincasCotiz = 0;
                    if (RJAfterEditCotizC.Count() >0) difincasCotiz = RJAfterEditCotizC.Sum(rjae => rjae.credit);
                    difplataCotiz = 0;
                    if (RJAfterEditCotizD.Count() > 0) difplataCotiz = RJAfterEditCotizD.Sum(rjae => rjae.credit);
                    difsoldcotiz = difincasCotiz - difplataCotiz;


                    decimal dif_ch_cotiz = chitanta.cotizatie - chitanteBeforeEdit.cotizatie;
                                       
                    if (chitanteBeforeEdit.cotizatie == 0)
                    {

                        if (chitanta.cotizatie == 0) { }//do nothing// 


                        if (chitanta.cotizatie != 0)
                        {

                            _context.registru_jurnal.Add(new registru_jurnal
                            {
                                data = chitanta.data,
                                id_document = chitanta.idchitanta,
                                nr_document = chitanta.nrch.ToString(),
                                id_CS_debitor = ID_CS_editCH,
                                id_CA_debitor = ID_CA_editCH,
                                id_CS_creditor = 101,
                                id_CA_creditor = Int32.Parse("113" + chitanta.nrcarnet.ToString()),
                                explicatie_nr_cont_analitic = "-",
                                SI_CS_debit = 0,
                                SI_CA_debit = 0,
                                SI_CS_credit = 0,
                                SI_CA_credit = pensEditChPost.soldcotiz- difsoldcotiz,
                                debit = chitanta.cotizatie,
                                credit = chitanta.cotizatie,
                                SF_CS_debit = 0,
                                SF_CA_debit = 0,
                                SF_CS_credit = 0,
                                SF_CA_credit = pensEditChPost.soldcotiz- difsoldcotiz + chitanta.cotizatie,
                                tip_document = chitanta.tip_document,
                                sortare = "H"

                            });

                            pensEditChPost.soldcotiz = pensEditChPost.soldcotiz + chitanta.cotizatie;

                            await _context.SaveChangesAsync();
                        }
                    }


                    if (chitanteBeforeEdit.cotizatie != 0)
                    {

                        var RJAfterEditCotizID = await _context.registru_jurnal
                                   .Where(rjaecid => (rjaecid.id_document == chitanta.idchitanta) && (rjaecid.id_CS_creditor == 101)).SingleOrDefaultAsync();

                        if (platac==1)
                             RJAfterEditCotizID = await _context.registru_jurnal
                                   .Where(rjaecid => (rjaecid.id_document == chitanta.idchitanta) && (rjaecid.id_CS_debitor == 101)).SingleOrDefaultAsync();

                        if (chitanta.cotizatie == 0)
                        {
                            if (RJAfterEditCotizID != null)
                                _context.registru_jurnal.Remove(RJAfterEditCotizID);

                            pensEditChPost.soldcotiz = pensEditChPost.soldcotiz - chitanteBeforeEdit.cotizatie;

                            await _context.SaveChangesAsync();
                        }

                        if (chitanta.cotizatie != 0)
                        {

                            if (RJAfterEditCotizID != null)
                            {

                              
                                RJAfterEditCotizID.credit = chitanta.cotizatie;
                                RJAfterEditCotizID.debit = chitanta.cotizatie;

                                if (chitanta.cotizatie < 0)
                                {
                                    RJAfterEditCotizID.credit = -chitanta.cotizatie;
                                    RJAfterEditCotizID.debit = -chitanta.cotizatie;
                                }


                                RJAfterEditCotizID.SF_CA_credit = RJAfterEditCotizID.SF_CA_credit + dif_ch_cotiz;

                                pensEditChPost.soldcotiz = pensEditChPost.soldcotiz + dif_ch_cotiz;

                                await _context.SaveChangesAsync();

                            }


                        }


                    }

                    foreach (registru_jurnal rjae in RJAfterEditCotizC)
                    {
                        rjae.SI_CA_credit = rjae.SI_CA_credit + dif_ch_cotiz;
                        rjae.SF_CA_credit = rjae.SF_CA_credit + dif_ch_cotiz;

                    }


                    foreach (registru_jurnal rjae in RJAfterEditCotizD)
                    {
                        rjae.SI_CA_debit = rjae.SI_CA_debit + dif_ch_cotiz;
                        rjae.SF_CA_debit = rjae.SF_CA_debit + dif_ch_cotiz;

                    }

                    await _context.SaveChangesAsync();

                }

                //aj deces
                //aj deces

                if (chitanteBeforeEdit.ajdeces != chitanta.ajdeces)
                {

                    pensEditChPost.lunaajdeces = chitanta.lunaajdeces;
                    await _context.SaveChangesAsync();

                    idcontanalitic = Int32.Parse("114" + chitanta.nrcarnet.ToString());

                    var RJAfterEditAjdC = _context.registru_jurnal
                            .Where(rcae => (rcae.data > chitanta.data) && (rcae.id_CA_creditor == idcontanalitic));

                    var RJAfterEditAjdD = _context.registru_jurnal
                            .Where(rcae => (rcae.data > chitanta.data) && (rcae.id_CA_debitor == idcontanalitic));

                    var RJAfterEdit = await _context.registru_jurnal
                        .Where(rcae => (rcae.data > chitanta.data) && (rcae.id_CA_creditor == idcontanalitic)).ToListAsync();

                    dif_ch = chitanta.ajdeces - chitanteBeforeEdit.ajdeces;

                    decimal difsoldAjd = 0;
                    decimal difincasAjd = 0;
                    decimal difplataAjd = 0;
                    difincasAjd = 0;
                    if (RJAfterEditAjdC.Count() > 0) difincasAjd = RJAfterEditAjdC.Sum(rjae => rjae.credit);
                    difplataAjd = 0;
                    if (RJAfterEditAjdD.Count() > 0) difplataAjd = RJAfterEditAjdD.Sum(rjae => rjae.credit);
                    difsoldAjd = difincasAjd - difplataAjd;


                    if (chitanteBeforeEdit.ajdeces == 0)
                    {

                        if (chitanta.ajdeces == 0) { }//do nothing// 

                        if (chitanta.ajdeces != 0)
                        {
                            _context.registru_jurnal.Add(new registru_jurnal
                            {
                                data = chitanta.data,
                                id_document = chitanta.idchitanta,
                                nr_document = chitanta.nrch.ToString(),
                                id_CS_debitor = ID_CS_editCH,
                                id_CA_debitor = ID_CA_editCH,
                                id_CS_creditor = 102,
                                id_CA_creditor = Int32.Parse("114" + pensEditChPost.nrcarnet.ToString()),
                                explicatie_nr_cont_analitic = "-",
                                SI_CS_debit = 0,
                                SI_CA_debit = 0,
                                SI_CS_credit = 0,
                                SI_CA_credit = pensEditChPost.soldajdeces - difsoldAjd,
                                debit = chitanta.ajdeces,
                                credit = chitanta.ajdeces,
                                SF_CS_debit = 0,
                                SF_CA_debit = 0,
                                SF_CS_credit = 0,
                                SF_CA_credit = pensEditChPost.soldajdeces-difsoldAjd + chitanta.ajdeces,
                                tip_document = chitanta.tip_document,
                                sortare = "H"

                            });

                            pensEditChPost.soldajdeces = pensEditChPost.soldajdeces + chitanta.ajdeces;

                            await _context.SaveChangesAsync();
                        }
                    }


                    if (chitanteBeforeEdit.ajdeces != 0)
                    {

                        var RJAfterEditID = await _context.registru_jurnal
                                   .Where(rcaecid => (rcaecid.id_document == chitanta.idchitanta) && (rcaecid.id_CS_creditor == 102)).SingleOrDefaultAsync();


                        if (plataajd == 1)
                            RJAfterEditID = await _context.registru_jurnal
                                  .Where(rjaecid => (rjaecid.id_document == chitanta.idchitanta) && (rjaecid.id_CS_debitor == 102)).SingleOrDefaultAsync();


                        if (chitanta.ajdeces == 0)
                        {

                            _context.registru_jurnal.Remove(RJAfterEditID);

                            pensEditChPost.soldajdeces = pensEditChPost.soldajdeces - chitanteBeforeEdit.ajdeces;

                            await _context.SaveChangesAsync();
                        }


                        if (chitanta.ajdeces != 0)
                        {
                            if (RJAfterEditID != null)
                            {

                            RJAfterEditID.debit = chitanta.ajdeces;
                            RJAfterEditID.credit = chitanta.ajdeces;
                            RJAfterEditID.SF_CA_credit = RJAfterEditID.SF_CA_credit + dif_ch;

                            pensEditChPost.soldajdeces = pensEditChPost.soldajdeces + dif_ch;

                            await _context.SaveChangesAsync();
                            }
                        }
                    }

                    foreach (registru_jurnal rcae in RJAfterEditAjdC)
                    {
                        rcae.SI_CA_credit = rcae.SI_CA_credit + dif_ch;
                        rcae.SF_CA_credit = rcae.SF_CA_credit + dif_ch;


                    }

                    foreach (registru_jurnal rcae in RJAfterEditAjdD)
                    {
                        rcae.SI_CA_credit = rcae.SI_CA_credit + dif_ch;
                        rcae.SF_CA_credit = rcae.SF_CA_credit + dif_ch;


                    }

                    await _context.SaveChangesAsync();

                }

                //taxa inscr                                        

                if (chitanteBeforeEdit.taxainscr != chitanta.taxainscr)
                {

                    if (chitanteBeforeEdit.taxainscr == 0)
                    {

                        if (chitanta.taxainscr != 0)
                        {

                            _context.registru_jurnal.Add(new registru_jurnal
                            {
                                data = chitanta.data,
                                id_document = chitanta.idchitanta,
                                nr_document = chitanta.nrch.ToString(),
                                id_CS_debitor = ID_CS_editCH,
                                id_CA_debitor = ID_CA_editCH,
                                id_CS_creditor = 105,
                                id_CA_creditor = Int32.Parse("1012" + pensEditChPost.nrcarnet.ToString()),
                                explicatie_nr_cont_analitic = "-",
                                SI_CS_debit = 0,
                                SI_CA_debit = 0,
                                SI_CS_credit = 0,
                                SI_CA_credit = 0,
                                debit = chitanta.taxainscr,
                                credit = chitanta.taxainscr,
                                SF_CS_debit = 0,
                                SF_CA_debit = 0,
                                SF_CS_credit = 0,
                                SF_CA_credit = 0,
                                tip_document = chitanta.tip_document,
                                sortare = "H"

                            });


                            await _context.SaveChangesAsync();
                        }
                    }




                    if (chitanteBeforeEdit.taxainscr != 0)
                    {
                        var RJAfterEditIDTx = await _context.registru_jurnal
                                     .Where(rjaecidtx => (rjaecidtx.id_document == chitanta.idchitanta) && (rjaecidtx.id_CS_creditor == 105)).SingleOrDefaultAsync();

                        if (chitanta.taxainscr == 0)
                        {
                            _context.registru_jurnal.Remove(RJAfterEditIDTx);
                            await _context.SaveChangesAsync();
                        }


                        if (chitanta.taxainscr != 0)
                        {
                            if (RJAfterEditIDTx != null)
                            {
                                RJAfterEditIDTx.credit = chitanta.taxainscr;
                                RJAfterEditIDTx.debit = chitanta.taxainscr;

                                await _context.SaveChangesAsync();

                            }


                        }
                    }

                }


                if (chitanteBeforeEdit.nrch != chitanta.nrch)
                {

                    var RJAterEditIDNrC = await _context.registru_jurnal
                                 .Where(rcaecidnc => (rcaecidnc.id_document == chitanta.idchitanta)).ToListAsync();


                    foreach (registru_jurnal rjaecidnc in RJAterEditIDNrC)
                    {
                        rjaecidnc.nr_document = chitanta.nrch.ToString();
                    }

                    await _context.SaveChangesAsync();


                }


                //rata
                if ((chitanta.tip_document!=2)  || (chitanta.tip_document != 4))
                if (chitanteBeforeEdit.rata != chitanta.rata)
                {
                    //cek if ch de editat nu are credit
                    //cek if NU if befe=re edit rata=0
                    //error if mai exista in rj 2678 debitor sau 2678 creditor cu data mai mare
                    //eror if dif rata > credit pay chbefore
                   
                    // if beforeedit<>0 si afteredit=0 cek update creditimpr si soldimp si rataplataitadesf 
                                     //+del din rj 106 si 103

                    //face update rj si creditimprumut cu diferenta de credit- if X

                    //msg err if are debit ch

                    var pensEditRata = await _context.Pensionars.Where(per => per.nrcarnet == chitanta.nrcarnet).SingleOrDefaultAsync();

                    idcontanalitic = Int32.Parse("2678" + chitanta.nrcarnet.ToString());

                    var RJAfterEditRataC = _context.registru_jurnal
                            .Where(rcae => (rcae.data > chitanta.data) && (rcae.id_CA_creditor == idcontanalitic));


                    int testCH = 0;
                    testCH = 0;
                    var cekrjEditRata=await _context.registru_jurnal.Where(crjer=>(crjer.data>chitanta.data) && ((crjer.id_CA_debitor==idcontanalitic) || (crjer.id_CA_creditor== idcontanalitic)) ).ToListAsync();
                    if (cekrjEditRata.Count() > 0) testCH = 1;                                          
                    if (testCH==1)
                    {
                      ViewData["Error"] = "Nu se poate face editare pentru rata decat daca exista chitanta cu rata cu data mai mare";
                      return View("Error");
                    }

                    idcontanalitic = Int32.Parse("4622" + chitanta.nrcarnet.ToString());
                    var cekrjEditRata106 = await _context.registru_jurnal.Where(crjer => (crjer.id_CA_creditor == idcontanalitic) && (crjer.id_document == chitanta.idchitanta)).SingleOrDefaultAsync();
                    if (cekrjEditRata106 == null)
                    {
                        ViewData["Error"] = "Nu se poate face editare pentru rata decat pentru partide care au credit imprumut";
                        return View("Error");
                    }

                    idcontanalitic = Int32.Parse("4612" + chitanta.nrcarnet.ToString());
                    var cekrjEditRata107 = await _context.registru_jurnal.Where(crjer => (crjer.id_CA_creditor == idcontanalitic) && (crjer.id_document == chitanta.idchitanta)).SingleOrDefaultAsync();
                    if (cekrjEditRata107 != null)
                    {
                        ViewData["Error"] = "Nu se poate face editare pentru rata daca chitanta a avut incasare pe debit";
                        return View("Error");
                    }

                    decimal dif_ch_rata = chitanta.rata - chitanteBeforeEdit.rata;

                    if (dif_ch_rata<0)
                    if (cekrjEditRata106.debit<-dif_ch_rata)
                    {
                       ViewData["Error"] = "Nu se poate face editare pentru rata decat pentru partide care au credit imprumut";
                       return View("Error");
                    }


                    if (chitanteBeforeEdit.rata == 0)
                    {
                        ViewData["Error"] = "Nu se poate face editare pentru rata decat daca nu exista rata introdusa inainte";
                        return View("Error");
                    }                                      

                    //pa cek code

                    if (chitanteBeforeEdit.rata != 0)
                    {
                        if (chitanta.rata==0)
                        {
                            var RJeditRata =await _context.registru_jurnal.Where(rjer => (rjer.id_document == chitanta.idchitanta) && (rjer.id_CS_creditor==106)).SingleOrDefaultAsync();
                            pensEditRata.credit_imprumut = pensEditRata.credit_imprumut - RJeditRata.credit;
                            RJeditRata = await _context.registru_jurnal.Where(rjer => (rjer.id_document == chitanta.idchitanta) && (rjer.id_CS_creditor == 103)).SingleOrDefaultAsync();
                            
                            if (RJeditRata !=null)
                            {
                             pensEditRata.soldimp = pensEditRata.soldimp + RJeditRata.credit;
                             pensEditRata.restimp = pensEditRata.restimp - RJeditRata.credit;
                             
                             await _context.SaveChangesAsync();
                            }

                            var desfEditRata = await _context.desfasurator_rate.Where(der => (der.nrcarnet==chitanta.nrcarnet) && (der.data_rata.Month == DateTime.Now.Month) && (der.data_rata.Year == DateTime.Now.Year)).SingleOrDefaultAsync(); 
                            if (RJeditRata !=null)
                            {
                             _context.Remove(await _context.registru_jurnal.Where(rjer => (rjer.id_document == chitanta.idchitanta) && (rjer.id_CS_creditor == 103)).SingleOrDefaultAsync());
                             desfEditRata.rata_platita = 0; 
                            }
                            
                            _context.Remove(await _context.registru_jurnal.Where(rjer => (rjer.id_document == chitanta.idchitanta) && (rjer.id_CS_creditor == 106)).SingleOrDefaultAsync());
                            await _context.SaveChangesAsync();
                        }


                        //if X
                        if (chitanta.rata != 0)
                        {
                            cekrjEditRata106.credit = cekrjEditRata106.credit + dif_ch_rata;
                            cekrjEditRata106.debit = cekrjEditRata106.debit + dif_ch_rata;
                            pensEditRata.credit_imprumut = pensEditRata.credit_imprumut + dif_ch_rata;
                            await _context.SaveChangesAsync();
                        }

                        //
                    }

                   
                }


                if ((chitanta.tip_document == 2) || (chitanta.tip_document == 4))
                {
                    int testCHd = 0;
                    testCHd = 0;
                    int idcontanaliticD;

                    var pensEditRataD = await _context.Pensionars.Where(per => per.nrcarnet == chitanta.nrcarnet).SingleOrDefaultAsync();

                    idcontanaliticD = pensEditRataD.nrcarnet;

                    var cekrjEditRataD = await _context.registru_jurnal.Where(crjer => (crjer.data > chitanta.data) && ((crjer.id_CA_debitor == idcontanaliticD) || (crjer.id_CA_creditor == idcontanaliticD))).ToListAsync();
                    if (cekrjEditRataD.Count() > 0) testCHd = 1;
                    if (testCHd == 1)
                    {
                        ViewData["Error"] = "Nu se poate face editare pentru rata decat daca exista chitanta cu rata cu data mai mare";
                        return View("Error");
                    }



                    if (chitanteBeforeEdit.rata == 0)
                    {
                        ViewData["Error"] = "Nu se poate face editare pentru rata decat daca nu exista rata introdusa inainte";
                        return View("Error");
                    }

                    decimal difrataD;
                    difrataD = chitanta.rata - chitanteBeforeEdit.rata;

                    if (chitanteBeforeEdit.rata != 0)
                    {
                        if (chitanta.rata==0)
                        {
                        var RJeditrataD = await _context.registru_jurnal.Where(rjerd => (rjerd.id_document == chitanta.idchitanta) && (rjerd.id_CA_creditor==idcontanaliticD)).SingleOrDefaultAsync();
                        _context.Remove(await _context.registru_jurnal.Where(rjerd => (rjerd.id_document == chitanta.idchitanta) && (rjerd.id_CA_creditor == idcontanaliticD)).SingleOrDefaultAsync());
                        pensEditRataD.sold_461 = pensEditRataD.sold_461 - chitanta.rata;
                        await _context.SaveChangesAsync();                        
                        }

                        if (chitanta.rata != 0)
                        {
                            var RJeditrataD = await _context.registru_jurnal.Where(rjerd => (rjerd.id_document == chitanta.idchitanta) && (rjerd.id_CA_creditor == idcontanaliticD)).SingleOrDefaultAsync();
                            RJeditrataD.credit = chitanta.rata ;
                            pensEditRataD.sold_461 = pensEditRataD.sold_461 -difrataD;
                            await _context.SaveChangesAsync();
                        }
                    }            

                }

                if (chitanteBeforeEdit.dobanda != chitanta.dobanda)
                {
                    //cek msgerr if chafteredit<>0
                    //cek msg err if nu contine credit 
                    //del rj dobanda si update sold
                    //msg err idf data mai mare am idcadebit si idcacredit 

                    if (chitanta.dobanda != 0)
                    {
                        ViewData["Error"] = "Nu se poate face decat anulari de chitante pentru editare dobanda";
                        return View("Error");
                    }


                    idcontanalitic = Int32.Parse("6791" + chitanta.nrcarnet.ToString());
                    var pensEditDobanda = await _context.Pensionars.Where(per => per.nrcarnet == chitanta.nrcarnet).SingleOrDefaultAsync();


                    if (chitanta.dobanda == 0)
                    {
                        var RJeditDobanda106 = await _context.registru_jurnal.Where(rjer => (rjer.id_document == chitanta.idchitanta) && (rjer.id_CS_creditor == 106)).SingleOrDefaultAsync();
                        if (RJeditDobanda106 == null)
                        {
                            ViewData["Error"] = "Nu se poate face editare dobanda decat daca chitanta nu are incasare pe credit";
                            return View("Error");
                        }

                        int testCHD = 0;
                        var cekrjEditDobanda = await _context.registru_jurnal.Where(crjer => (crjer.data > chitanta.data) && ((crjer.id_CA_debitor == idcontanalitic) || (crjer.id_CA_creditor == idcontanalitic))).ToListAsync();
                        if (cekrjEditDobanda.Count() > 0) testCHD = 1;
                        if (testCHD == 1)
                        {
                            ViewData["Error"] = "Nu se poate face editare pentru dobanda daca exista chitanta cu dobanda cu data mai mare";
                            return View("Error");
                        }

                        //ch now


                        var RJeditDobanda = await _context.registru_jurnal.Where(rjer => (rjer.id_document == chitanta.idchitanta) && (rjer.id_CS_creditor == 109)).SingleOrDefaultAsync();
                        _context.Remove(RJeditDobanda);
                        _context.Remove(RJeditDobanda = await _context.registru_jurnal.Where(rjer => (rjer.id_document == chitanta.idchitanta) && (rjer.id_CS_creditor == 54)).SingleOrDefaultAsync());

                        pensEditDobanda.solddobanda = pensEditDobanda.solddobanda + chitanteBeforeEdit.dobanda;
                        await _context.SaveChangesAsync();

                    }
                }

                try
                {
                    _context.Update(chitanta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChitantaExists(chitanta.idchitanta))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }


                insert_audit_chitanta(getiduserch.IDUser, chitanta.idchitanta, chitanta.nrch, chitanta.data, chitanta.nrcarnet, chitanta.rata, chitanta.dobanda, chitanta.cotizatie, chitanta.ajdeces, chitanta.lunaajdeces, "dupa");
                insert_audit_chitanta(getiduserch.IDUser, chitanta.idchitanta, chitanteBeforeEdit.nrch, chitanteBeforeEdit.data, chitanteBeforeEdit.nrcarnet, chitanteBeforeEdit.rata, chitanteBeforeEdit.dobanda, chitanteBeforeEdit.cotizatie, chitanteBeforeEdit.ajdeces, chitanteBeforeEdit.lunaajdeces, "inainte");



                return RedirectToAction("Index", new { nrcarnet = chitanta.nrcarnet });
            }
            return View(chitanta);
        }
             

        private bool ChitantaExists(int id)
        {
            return _context.Chitantas.Any(e => e.idchitanta == id);
        }

        public int update_registru_casa(int id_ch, int nr_ch, int nrcarnet, DateTime datach, decimal rata_de_plata, decimal rata, decimal dobanda, decimal cotizatie, decimal taxainscr, string nume, decimal ajdeces, decimal dobanda_penalizatoare)
        {
            // Iunie 2024 tablete

            string tip_suma;
            tip_suma = "incasare";
            int esteIncasare = 1;
            string sub2001 = "-";
            decimal dif_rata = 0;
            int idcredit = 0, iddebit = 0;
            string nrdoc = "";
            int iddp = 0;
            int idd = 0;


            nrdoc = nr_ch.ToString();
            if (ajdeces < 0) nrdoc = "_nr.inreg._aj.dec.";

            var pensrc = _context.Pensionars
                .Where(prc => prc.nrcarnet == nrcarnet).SingleOrDefault();


            if (cotizatie < 0)
            {

                tip_suma = "plata";
                esteIncasare = -1;
            }

            if (ajdeces < 0)
            {

                tip_suma = "plata";
                esteIncasare = -1;
            }

            if (taxainscr != 0)
            {
                _context.registru_casa.Add(new registru_casa
                {
                    data = datach,
                    nr_document = nr_ch.ToString(),
                    id_chimpr = id_ch,
                    id_cont_sintetic = 105,
                    id_cont_analitic = Int32.Parse("1012" + nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    sold_initial = 0,
                    suma = taxainscr,
                    incasare = taxainscr,
                    plata = 0,
                    sold_final = 0,
                    tip = "incasare",
                    sortare = "H"
                });

                _context.SaveChanges();
            }


            if (cotizatie > 0)
            {
                _context.registru_casa.Add(new registru_casa
                {
                    data = datach,
                    nr_document = nr_ch.ToString(),
                    id_chimpr = id_ch,
                    id_cont_sintetic = 101,
                    id_cont_analitic = Int32.Parse("113" + nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    sold_initial = pensrc.soldcotiz - cotizatie,
                    suma = cotizatie,
                    incasare = cotizatie,
                    plata = 0,
                    sold_final = pensrc.soldcotiz,
                    tip = "incasare",
                    sortare = "H"

                });

                _context.SaveChanges();
            }


            if (cotizatie < 0)
            {
                _context.registru_casa.Add(new registru_casa
                {
                    data = datach,
                    nr_document = nrdoc + nr_ch.ToString(),
                    id_chimpr = id_ch,
                    id_cont_sintetic = 101,
                    id_cont_analitic = Int32.Parse("113" + nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    sold_initial = pensrc.soldcotiz - cotizatie,
                    suma = -cotizatie,
                    incasare = 0,
                    plata = -cotizatie,
                    sold_final = pensrc.soldcotiz,
                    tip = "plata",
                    sortare = "B"

                });

                _context.SaveChanges();
            }


            if (ajdeces > 0)
            {
                _context.registru_casa.Add(new registru_casa
                {
                    data = datach,
                    nr_document = nr_ch.ToString(),
                    id_chimpr = id_ch,
                    id_cont_sintetic = 102,
                    id_cont_analitic = Int32.Parse("114" + nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = sub2001,
                    sold_initial = pensrc.soldajdeces - ajdeces,
                    suma = ajdeces,
                    incasare = ajdeces,
                    plata = 0,
                    sold_final = pensrc.soldajdeces,
                    tip = "incasare",
                    sortare = "H"
                });

                _context.SaveChanges();
            }

            if (ajdeces < 0)
            {
                _context.registru_casa.Add(new registru_casa
                {
                    data = datach,
                    nr_document = nrdoc + nr_ch.ToString(),
                    id_chimpr = id_ch,
                    id_cont_sintetic = 102,
                    id_cont_analitic = Int32.Parse("114" + nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = sub2001,
                    sold_initial = pensrc.soldajdeces - ajdeces,
                    suma = -ajdeces,
                    incasare = 0,
                    plata = -ajdeces,
                    sold_final = pensrc.soldajdeces,
                    tip = "plata",
                    sortare = "B"
                });

                _context.SaveChanges();
            }



            if (dobanda != 0)
            {
                _context.registru_casa.Add(new registru_casa
                {
                    data = datach,
                    nr_document = nr_ch.ToString(),
                    id_chimpr = id_ch,
                    id_cont_sintetic = 109,
                    id_cont_analitic = Int32.Parse("6791" + nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    sold_initial = pensrc.solddobanda + dobanda,
                    suma = dobanda,
                    incasare = dobanda,
                    plata = 0,
                    sold_final = pensrc.solddobanda,
                    tip = "incasare",
                    sortare = "H"
                });

                _context.SaveChanges();


                idd = Int32.Parse("6791" + nrcarnet.ToString());

                var checkIdDP = _context.conturi_analitice
                .Where(cic => cic.id_cont_analitic == idd);

                if (checkIdDP.Count() == 0)
                {
                    _context.conturi_analitice.Add(new conturi_analitice
                    {
                        id_cont_analitic = Int32.Parse("6791" + pensrc.nrcarnet.ToString()),
                        id_cont_sintetic = 109,
                        nr_cont_analitic = "26791." + pensrc.nrcarnet.ToString(),
                        explicatie_nr_cont_analitic = pensrc.nume
                    });
                    _context.SaveChanges();
                }

            }



            if (dobanda_penalizatoare != 0)
            {
                _context.registru_casa.Add(new registru_casa
                {
                    data = datach,
                    nr_document = nr_ch.ToString(),
                    id_chimpr = id_ch,
                    id_cont_sintetic = 108,
                    id_cont_analitic = Int32.Parse("6792" + nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    sold_initial = pensrc.sold_DP,
                    suma = dobanda_penalizatoare,
                    incasare = dobanda_penalizatoare,
                    plata = 0,
                    sold_final = pensrc.sold_DP - dobanda_penalizatoare,
                    tip = "incasare",
                    sortare = "H"
                });

                _context.SaveChanges();

                iddp = Int32.Parse("6792" + nrcarnet.ToString());

                var checkIdDP = _context.conturi_analitice
                .Where(cic => cic.id_cont_analitic == iddp);

                if (checkIdDP.Count() == 0)
                {
                    _context.conturi_analitice.Add(new conturi_analitice
                    {
                        id_cont_analitic = Int32.Parse("6792" + pensrc.nrcarnet.ToString()),
                        id_cont_sintetic = 108,
                        nr_cont_analitic = "26792." + pensrc.nrcarnet.ToString(),
                        explicatie_nr_cont_analitic = pensrc.nume
                    });
                    _context.SaveChanges();
                }

            }


            return 0;


        }

        public int update_registru_jurnal(int idch_rj, int nrch_rj, int nrcarnet_rj, DateTime datach_rj, decimal dobanda_rj, decimal cotizatie_rj, decimal taxainscr_rj, decimal ajdeces_rj, decimal dp_rj, int tip_document_rj)
        {
            //IUNIE 2024 TABLETE-20 mai

            //DE FACUT
            //i) bagat idch
            //ii) creat id 51 cont casa , id 52 cont banca, ida 101 ca casa, ida 102 ca banca
            //ii) solutie la nrdocument poate ajunge si la 30 char (ca idee un id ceva...)
            //iv) la crerare tabel not allow null la all- nu am pus required la model
            //tip_document=0 pt ch, 1 pt ncb, 2 pt ...

            int idcredit = 0, iddebit = 0;
            int id_CS = 0, id_CA = 0;
            int id_CA_debit = 0, id_CA_credit = 0;
            int get_iduser_rj = 0;
            string nrdoc = "";


            var pensrj = _context.Pensionars
                .Where(prj => prj.nrcarnet == nrcarnet_rj).SingleOrDefault();

            id_CS = 51;
            id_CA = 101;

            string userIdrj = User.Identity.Name.Trim();
            var get_iduser_rj_db = _context.utilizatoris
              .Where(u => u.nume_user == userIdrj).SingleOrDefault();

            get_iduser_rj = get_iduser_rj_db.IDUser;
         



            if (get_iduser_rj == 16)
            {
                id_CS = 51;
                id_CA = 151;
            }

            if (get_iduser_rj == 17)
            {
                id_CS = 51;
                id_CA = 152;
            }

            if (get_iduser_rj == 19)
            {
                id_CS = 51;
                id_CA = 154;
            }

            if (get_iduser_rj == 18)
            {
                id_CS = 51;
                id_CA = 153;
            }


            if (get_iduser_rj == 20)
            {
                id_CS = 51;
                id_CA = 155;
            }

            if (get_iduser_rj == 22)
            {
                id_CS = 51;
                id_CA = 156;
            }

            //banca
            if (tip_document_rj == 1)
            {
                id_CS = 52;
                id_CA = 102;
            }


            // deaici nc2
            //test if nu a predat idca , test after a predat idca
            //test nc 2 normala nu deleg
            

            if ((tip_document_rj == 51) || (tip_document_rj == 52) || (tip_document_rj == 5))
            {

                var exista_CA = _context.conturi_analitice.Where(eca => eca.nr_cont_analitic.Trim() == "4621." + nrcarnet_rj.ToString()).SingleOrDefault();

                if (exista_CA == null)
                {
                    _context.conturi_analitice.Add(new conturi_analitice
                    {
                        id_cont_analitic = Int32.Parse("4621" + nrcarnet_rj.ToString()),
                        id_cont_sintetic = 18,
                        nr_cont_analitic = "4621." + nrcarnet_rj.ToString(),
                        explicatie_nr_cont_analitic = pensrj.nume,
                    });

                    _context.SaveChanges();

                    id_CA = Int32.Parse("4621" + nrcarnet_rj.ToString());
                }


                id_CS = 18;
                if (exista_CA != null)
                    id_CA = exista_CA.id_cont_analitic;


            }





            if (taxainscr_rj != 0)
            {
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rj,
                    id_document = idch_rj,
                    nr_document = nrch_rj.ToString(),
                    id_CS_debitor = id_CS,
                    id_CA_debitor = id_CA,
                    id_CS_creditor = 105,
                    id_CA_creditor = Int32.Parse("1012" + nrcarnet_rj.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = taxainscr_rj,
                    credit = taxainscr_rj,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();
            }


            if (cotizatie_rj > 0)
            {
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rj,
                    id_document = idch_rj,
                    nr_document = nrch_rj.ToString(),
                    id_CS_debitor = id_CS,
                    id_CA_debitor = id_CA,
                    id_CS_creditor = 101,
                    id_CA_creditor = Int32.Parse("113" + nrcarnet_rj.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pensrj.soldcotiz - cotizatie_rj,
                    debit = cotizatie_rj,
                    credit = cotizatie_rj,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pensrj.soldcotiz,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();
            }

            //pa  code mai trebuie !!!! add new 

            if (tip_document_rj == 6)
                if (cotizatie_rj < 0)
                {
                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = datach_rj,
                        id_document = idch_rj,
                        nr_document = nrch_rj.ToString(),
                        id_CS_debitor = id_CS,
                        id_CA_debitor = id_CA,
                        id_CS_creditor = 101,
                        id_CA_creditor = Int32.Parse("113" + nrcarnet_rj.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = pensrj.soldcotiz - cotizatie_rj,
                        SI_CS_credit = 0,
                        SI_CA_credit = 0,
                        debit = cotizatie_rj,
                        credit = cotizatie_rj,
                        SF_CS_debit = 0,
                        SF_CA_debit = pensrj.soldcotiz,
                        SF_CS_credit = 0,
                        SF_CA_credit = 0,
                        tip_document = tip_document_rj,
                        sortare = "H"

                    });

                    if (tip_document_rj == 5) pensrj.id_stare = 2;

                    _context.SaveChanges();
                }


            nrdoc = "";
            if (ajdeces_rj < 0) nrdoc = "_nr.inreg._aj.dec.";

            if (tip_document_rj != 6)
                if (cotizatie_rj < 0)
                {
                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = datach_rj,
                        id_document = idch_rj,
                        nr_document = nrdoc + nrch_rj.ToString(),
                        id_CS_debitor = 101,
                        id_CA_debitor = Int32.Parse("113" + nrcarnet_rj.ToString()),
                        id_CS_creditor = id_CS,
                        id_CA_creditor = id_CA,
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = pensrj.soldcotiz - cotizatie_rj,
                        SI_CS_credit = 0,
                        SI_CA_credit = 0,
                        debit = -cotizatie_rj,
                        credit = -cotizatie_rj,
                        SF_CS_debit = 0,
                        SF_CA_debit = pensrj.soldcotiz,
                        SF_CS_credit = 0,
                        SF_CA_credit = 0,
                        tip_document = tip_document_rj,
                        sortare = "B"

                    });

                    if (tip_document_rj == 5) pensrj.id_stare = 2;

                    _context.SaveChanges();
                }



            if (ajdeces_rj > 0)
            {

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rj,
                    id_document = idch_rj,
                    nr_document = nrch_rj.ToString(),
                    id_CS_debitor = id_CS,
                    id_CA_debitor = id_CA,
                    id_CS_creditor = 102,
                    id_CA_creditor = Int32.Parse("114" + nrcarnet_rj.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pensrj.soldajdeces - ajdeces_rj,
                    debit = ajdeces_rj,
                    credit = ajdeces_rj,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pensrj.soldajdeces,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();
            }

            if (tip_document_rj != 6)
                if (ajdeces_rj < 0)
                {
                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = datach_rj,
                        id_document = idch_rj,
                        nr_document = "_nr.inreg._aj.dec." +  nrch_rj.ToString(),
                        id_CS_debitor = 102,
                        id_CA_debitor = Int32.Parse("114" + nrcarnet_rj.ToString()),
                        id_CS_creditor = id_CS,
                        id_CA_creditor = id_CA,
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = pensrj.soldajdeces - ajdeces_rj,
                        SI_CS_credit = 0,
                        SI_CA_credit = 0,
                        debit = -ajdeces_rj,
                        credit = -ajdeces_rj,
                        SF_CS_debit = 0,
                        SF_CA_debit = pensrj.soldajdeces,
                        SF_CS_credit = 0,
                        SF_CA_credit = 0,
                        tip_document = tip_document_rj,
                        sortare = "B"

                    });

                    _context.SaveChanges();
                }


            if (tip_document_rj == 6)
                if (ajdeces_rj < 0)
                {
                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = datach_rj,
                        id_document = idch_rj,
                        nr_document = nrch_rj.ToString(),
                        id_CS_debitor = id_CS,
                        id_CA_debitor = id_CA,
                        id_CS_creditor = 102,
                        id_CA_creditor = Int32.Parse("114" + nrcarnet_rj.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = pensrj.soldajdeces - ajdeces_rj,
                        SI_CS_credit = 0,
                        SI_CA_credit = 0,
                        debit = ajdeces_rj,
                        credit = ajdeces_rj,
                        SF_CS_debit = 0,
                        SF_CA_debit = pensrj.soldajdeces,
                        SF_CS_credit = 0,
                        SF_CA_credit = 0,
                        tip_document = tip_document_rj,
                        sortare = "H"

                    });

                    _context.SaveChanges();
                }



            if (tip_document_rj != 4)
                if (dobanda_rj != 0)
            {

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rj,
                    id_document = idch_rj,
                    nr_document = nrch_rj.ToString(),
                    id_CS_debitor = id_CS,
                    id_CA_debitor = id_CA,
                    id_CS_creditor = 109,
                    id_CA_creditor = Int32.Parse("6791" + nrcarnet_rj.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pensrj.solddobanda + dobanda_rj,
                    debit = dobanda_rj,
                    credit = dobanda_rj,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pensrj.solddobanda,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rj,
                    id_document = idch_rj,
                    nr_document = nrch_rj.ToString(),
                    id_CS_debitor = 53,
                    id_CA_debitor = 103,
                    id_CS_creditor = 54,
                    id_CA_creditor = 104,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = dobanda_rj,
                    credit = dobanda_rj,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();
            }


            if (tip_document_rj!=4)
            if (dp_rj != 0)
            {
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rj,
                    id_document = idch_rj,
                    nr_document = nrch_rj.ToString(),
                    id_CS_debitor = id_CS,
                    id_CA_debitor = id_CA,
                    id_CS_creditor = 108,
                    id_CA_creditor = Int32.Parse("6792" + nrcarnet_rj.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pensrj.sold_DP,
                    debit = dp_rj,
                    credit = dp_rj,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pensrj.sold_DP - dp_rj,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                pensrj.sold_DP = pensrj.sold_DP - dp_rj;

                _context.SaveChanges();

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rj,
                    id_document = idch_rj,
                    nr_document = nrch_rj.ToString(),
                    id_CS_debitor = 53,
                    id_CA_debitor = 103,
                    id_CS_creditor = 54,
                    id_CA_creditor = 104,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = dp_rj,
                    credit = dp_rj,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();
            }



            return 0;

        }

        public int update_pensionar_chit(int nrcarnet, decimal rata_de_plata, decimal rata, decimal cotizatie, DateTime ultima_plata, DateTime lunaajdeces, string tipsql, decimal ajd, decimal dob, decimal taxai, int tip_ch, int nrchu)
        {
            //IUNIE 2024 TABLETE
            int test_sld_imp_0 = 0;
            string nrcarnetPD = "";
            int nrcarnetPDint = 0;

            var pens = (from p in _context.Pensionars
                        where ((p.nrcarnet == nrcarnet))
                        select p).FirstOrDefault();

            pens.soldcotiz = pens.soldcotiz + cotizatie;

            if (pens.id_stare != 4)
            pens.solddobanda = pens.solddobanda - dob;

            //de aici
            if (pens.id_stare==4)
            {
            nrcarnetPD = pens.nrcarnet.ToString().Remove(0, 4);
            nrcarnetPDint = Int32.Parse(nrcarnetPD);
            var pens4611D = _context.Pensionars.Where(ppd => ppd.nrcarnet == nrcarnetPDint).SingleOrDefault();
            pens4611D.solddobanda = pens4611D.solddobanda - dob;
            }



            pens.soldajdeces = pens.soldajdeces + ajd;

            if ((pens.soldajdeces < 0) && (pens.id_stare == 0) && (pens.soldimp==0)) pens.id_stare = 1;


            if (tip_ch == 51) pens.id_stare = 51;
            if (tip_ch == 52) pens.id_stare = 52;

              if ((cotizatie < 0) && (ajd == 0)  && (tip_ch==0) && (nrchu!=1)) pens.id_stare = 52;

            if (tipsql == "C") if (rata > 0) pens.lunaimp = ultima_plata;
            if (tipsql == "C") pens.lunaajdeces = lunaajdeces;

            _context.SaveChanges();

            return 0;
        }

        public int update_nrch(int nrchitanta, string username)
        {
            var nrch = (from n in _context.nr_chitanta
                        where ((n.nume_user == username))
                        select n).FirstOrDefault();

            nrch.nrchitanta = nrchitanta;
            _context.SaveChanges();
            return 0;
        }

        public decimal calcul_dobanda_desfasurator(int nr_carnet_D, DateTime data_ch)
        {
            //IUNIE 2024 TABLETE
            decimal dobanda_desfasurator = 0, procentnou = 0;
            int i = 0, k = 0;
            string nrcarnetCDD = "0"; int nrcarnetCDDint = 0;

            DateTime data_last_ch = Convert.ToDateTime("1/1/1997");
            DateTime lunaimp = Convert.ToDateTime("1/1/1999");
            DateTime luna = Convert.ToDateTime("1/1/1995");

            var pens_desfasurator = _context.Pensionars
                .Where(pd => pd.nrcarnet == nr_carnet_D).SingleOrDefault();

            if (pens_desfasurator.id_stare == 3) return 0;



            if (pens_desfasurator.id_stare == 4)
            {
                nrcarnetCDD = pens_desfasurator.nrcarnet.ToString().Remove(0, 4);
                nrcarnetCDDint = Int32.Parse(nrcarnetCDD);
                var pens4611CDD = _context.Pensionars.Where(ppd => ppd.nrcarnet == nrcarnetCDDint).SingleOrDefault();
                return pens4611CDD.solddobanda;

            }

            // if (pens_desfasurator.id_stare == 4) return pens_desfasurator.solddobanda;
            if (pens_desfasurator.soldimp <= 0) return 0;



            lunaimp = pens_desfasurator.lunaimp;
            luna = lunaimp;
            //var ch_nrcarnet_d = (from c in db.chitantas
            //                   where ((c.nrcarnet == nr_carnet_D) && (c.rata > 0))
            //                 orderby c.data descending
            //               select c).FirstOrDefault();
            //if (ch_nrcarnet_d != null) data_last_ch = ch_nrcarnet_d.data;
            //if (data_last_ch >= lunaimp) luna = data_last_ch;

            //var nrrata_luna1 = db.desfasurator_rate
            //  .Where(dd => (dd.nrcarnet == nr_carnet_D) && (dd.data_rata.Month == luna.Month) && (dd.data_rata.Year == DateTime.Now.Year)).SingleOrDefault();

            //var nrrata_now = db.desfasurator_rate
            //  .Where(dd => (dd.nrcarnet == nr_carnet_D) && (dd.data_rata.Month == luna.Month) && (dd.data_rata.Year == DateTime.Now.Year)).SingleOrDefault();

            //            dobanda_desfasurator = db.desfasurator_rate
            //            .Where(dd => ((dd.nr_rata > nrrata_luna1.nr_rata) && (dd.nr_rata <= nrrata_now.nr_rata))).Sum(dd=>dd.dobanda);

            dobanda_desfasurator = 0;

            if ((luna.Month == data_ch.Month) && (luna.Year == data_ch.Year)) return 0;

            k = 0;
            i = 0;

            while (i == 0)
            {
                luna = luna.AddMonths(1);

                dobanda_desfasurator = dobanda_desfasurator +
                         _context.desfasurator_rate
                       .Where(dd => ((dd.nrcarnet == nr_carnet_D) && (dd.data_rata.Month == luna.Month) && (dd.data_rata.Year == luna.Year))).Sum(dd => dd.dobanda);

                //    if ((luna.Month == DateTime.Now.Month) && (luna.Year == DateTime.Now.Year)) i = 1;
                if ((luna.Month == data_ch.Month) && (luna.Year == data_ch.Year)) i = 1;
            }

            return (dobanda_desfasurator);

        }

        public decimal calcul_dobanda_penalizatoare(int nrcarnet_dp, DateTime datach_dp)
        {
            //iuniE 2024 Tablete

            decimal dp_desfasurator = 0;
            int i = 0, k = 0;

            DateTime lunaimp_dp = Convert.ToDateTime("1/1/1999");
            DateTime luna_dp = Convert.ToDateTime("1/1/1995");


            var pens_desfasurator_dp = _context.Pensionars
                .Where(pdp => pdp.nrcarnet == nrcarnet_dp).SingleOrDefault();

            if (pens_desfasurator_dp.id_stare == 3) return 0;
            if (pens_desfasurator_dp.id_stare == 4) return pens_desfasurator_dp.sold_DP;

            if (pens_desfasurator_dp.soldimp <= 0) return 0;

            lunaimp_dp = pens_desfasurator_dp.lunaimp;
            luna_dp = lunaimp_dp;

            dp_desfasurator = 0;

            if ((luna_dp.Month == datach_dp.Month) && (luna_dp.Year == datach_dp.Year)) return 0;

            k = 0;
            i = 0;

            while (i == 0)
            {
                luna_dp = luna_dp.AddMonths(1);

                dp_desfasurator = dp_desfasurator +
                         _context.desfasurator_rate
                       .Where(ddp => ((ddp.nrcarnet == nrcarnet_dp) && (ddp.data_rata.Month == luna_dp.Month) && (ddp.data_rata.Year == luna_dp.Year))).Sum(ddp => ddp.dobanda_penalizatoare);

                if ((luna_dp.Month == datach_dp.Month) && (luna_dp.Year == datach_dp.Year)) i = 1;
            }

         //   if (lunaimp_dp.Year == 2023)
           //     dp_desfasurator = dp_desfasurator + pens_desfasurator_dp.sold_DP2016;

            //if (luna_dp.Month > 1)
            //  dp_desfasurator = dp_desfasurator+ pens_desfasurator_dp.sold_DP2016;


            if (pens_desfasurator_dp.sold_DP < 0) dp_desfasurator = 0;

            return dp_desfasurator;

        }

        public int update_rj_debitori_p(int idch_rjdp, int nrch_rjdp, int nrcarnet_rjdp, DateTime datach_rjdp, decimal dobanda_rjdp, decimal dp_rjdp, decimal rata_rjdp, int tip_document_rjdp)
        {
            //iunie tablete 2024

            int id_CS_rjdp = 0;
            int id_CA_rjdp = 0;



            string id_ca_D_str = nrcarnet_rjdp.ToString().Trim();
            id_ca_D_str = id_ca_D_str.Remove(0, 4);
            int id_ca_D = Int32.Parse(id_ca_D_str);


            var pens_rjdp = _context.Pensionars
               .Where(prjdp => prjdp.nrcarnet == nrcarnet_rjdp).SingleOrDefault();

            id_CS_rjdp = 52;
            id_CA_rjdp = 102;

            if (dobanda_rjdp != 0)
            {
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rjdp,
                    id_document = idch_rjdp,
                    nr_document = nrch_rjdp.ToString(),
                    id_CS_debitor = id_CS_rjdp,
                    id_CA_debitor = id_CA_rjdp,
                    id_CS_creditor = 109,
                    id_CA_creditor = Int32.Parse("6791" + id_ca_D.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_rjdp.solddobanda + dobanda_rjdp,
                    debit = dobanda_rjdp,
                    credit = dobanda_rjdp,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_rjdp.solddobanda,
                    tip_document = tip_document_rjdp,
                    sortare = "H"

                });

                _context.SaveChanges();

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rjdp,
                    id_document = idch_rjdp,
                    nr_document = nrch_rjdp.ToString(),
                    id_CS_debitor = 53,
                    id_CA_debitor = 103,
                    id_CS_creditor = 54,
                    id_CA_creditor = 104,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = dobanda_rjdp,
                    credit = dobanda_rjdp,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = tip_document_rjdp,
                    sortare = "H"
                });

                _context.SaveChanges();
            }

            if (dp_rjdp != 0)
            {
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rjdp,
                    id_document = idch_rjdp,
                    nr_document = nrch_rjdp.ToString(),
                    id_CS_debitor = id_CS_rjdp,
                    id_CA_debitor = id_CA_rjdp,
                    id_CS_creditor = 108,
                    id_CA_creditor = Int32.Parse("6792" + id_ca_D.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_rjdp.sold_DP,
                    debit = dp_rjdp,
                    credit = dp_rjdp,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_rjdp.sold_DP - dp_rjdp,
                    tip_document = tip_document_rjdp,
                    sortare = "H"

                });

                pens_rjdp.sold_DP = pens_rjdp.sold_DP - dp_rjdp;

                _context.SaveChanges();


                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rjdp,
                    id_document = idch_rjdp,
                    nr_document = nrch_rjdp.ToString(),
                    id_CS_debitor = 53,
                    id_CA_debitor = 103,
                    id_CS_creditor = 54,
                    id_CA_creditor = 104,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = dp_rjdp,
                    credit = dp_rjdp,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = tip_document_rjdp,
                    sortare = "H"
                });

                _context.SaveChanges();
            }

            if (rata_rjdp != 0)
            {
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rjdp,
                    id_document = idch_rjdp,
                    nr_document = nrch_rjdp.ToString(),
                    id_CS_debitor = id_CS_rjdp,
                    id_CA_debitor = id_CA_rjdp,
                    id_CS_creditor = 27,
                    id_CA_creditor = nrcarnet_rjdp,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_rjdp.sold_461,
                    debit = rata_rjdp,
                    credit = rata_rjdp,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_rjdp.sold_461 - rata_rjdp,
                    tip_document = tip_document_rjdp,
                    sortare = "H"

                });

                pens_rjdp.sold_461 = pens_rjdp.sold_461 - rata_rjdp;

                _context.SaveChanges();

            }

            return 0;
        }

        public int update_rj_debitori_d(int idch_rjdd, int nrch_rjdd, int nrcarnet_rjdd, DateTime datach_rjdd, decimal rata_rjdd, int tip_document_rjdd, int userID_rjdd)
        {
            //iuniE tablete 2024
            int id_CS_rjdd = 0;
            int id_CA_rjdd = 0;

            var pens_rjdd = _context.Pensionars
               .Where(prjdd => prjdd.nrcarnet == nrcarnet_rjdd).SingleOrDefault();

            // var getIdCCA=db.conturi_analitice.Where(gicca=>gicca.nr_cont_analitic==)

            id_CS_rjdd = 51;
            id_CA_rjdd = 101;

            if (tip_document_rjdd == 3)
            {
                id_CS_rjdd = 52;
                id_CA_rjdd = 102;
            }


            if (userID_rjdd==16)
            {
                id_CS_rjdd = 51;
                id_CA_rjdd = 151;
            }

            if (userID_rjdd == 17)
            {
                id_CS_rjdd = 51;
                id_CA_rjdd = 152;
            }


            if (userID_rjdd == 19)
            {
                id_CS_rjdd = 51;
                id_CA_rjdd = 154;
            }


            if (userID_rjdd == 18)
            {
                id_CS_rjdd = 51;
                id_CA_rjdd = 153;
            }


            if (userID_rjdd== 20)
            {
                id_CS_rjdd = 51;
                id_CA_rjdd = 155;
            }

            if (userID_rjdd == 22)
            {
                id_CS_rjdd = 51;
                id_CA_rjdd = 156;
            }


            int  id_ca_4611 = nrcarnet_rjdd;

            if (rata_rjdd != 0)
            {
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = datach_rjdd,
                    id_document = idch_rjdd,
                    nr_document = nrch_rjdd.ToString(),
                    id_CS_debitor = id_CS_rjdd,
                    id_CA_debitor = id_CA_rjdd,
                    id_CS_creditor = 26,
                    id_CA_creditor = id_ca_4611,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_rjdd.sold_461,
                    debit = rata_rjdd,
                    credit = rata_rjdd,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_rjdd.sold_461 - rata_rjdd,
                    tip_document = tip_document_rjdd,
                    sortare = "H"

                });

                pens_rjdd.sold_461 = pens_rjdd.sold_461 - rata_rjdd;
                //pens_rjdd.restimp = pens_rjdd.restimp + rata_rjdd;
                _context.SaveChanges();

               
            }

            return 0;
        }

        public decimal utilizare_credit_rata_imprumut(int nr_carnet_use_c, int idch_use_c, int nrch_use_c, decimal rata_ch_de_plata_pt_use_c, decimal rata_ch_use_c, DateTime data_ch_use_c, decimal rata_platita_ch_use_c, decimal dobanda_ch_use_c, int tip_document_rj, decimal dp_ch_use)
        {
            // IUNIE 2025


            int test_month_now = 0;
            int idcredit = 0, iddebit = 0;
            int stop = 0;
            int check_lichidare = 0;
            decimal credit_var_uc = 0;
            decimal use_credit = 0, use_credit_initial = 0;
            decimal rata_var_uc = 0;
            decimal dif_rata_pt_use_c = 0;
            decimal iesire_462 = 0, iesire_2678 = 0, iesire_461 = 0, incasare_461 = 0;
            decimal incasare_2678 = 0, incasare_462 = 0, intrare_461 = 0;
            int test_new_record = 0;
            int test_NC = 0;
            int id_CS = 0;
            int id_CA = 0;
            int get_iduser_rj_2678 = 0;


            id_CS = 51;
            id_CA = 101;

            //delegati
            string userIdrj = User.Identity.Name.Trim();
            var get_iduser_rj__2678_db = _context.utilizatoris
               .Where(u => u.nume_user == userIdrj).SingleOrDefault();

            get_iduser_rj_2678 = get_iduser_rj__2678_db.IDUser;

            if (get_iduser_rj_2678 == 16)
            {
                id_CS = 51;
                id_CA = 151;
            }


            if (get_iduser_rj_2678 == 17)
            {
                id_CS = 51;
                id_CA = 152;
            }

            if (get_iduser_rj_2678 == 19)
            {
                id_CS = 51;
                id_CA = 154;
            }


            if (get_iduser_rj_2678 == 18)
            {
                id_CS = 51;
                id_CA = 153;
            }


            if (get_iduser_rj_2678 == 20)
            {
                id_CS = 51;
                id_CA = 155;
            }

            if (get_iduser_rj_2678 == 22)
            {
                id_CS = 51;
                id_CA = 156;
            }


            //banca
            if (tip_document_rj == 1)
            {
                id_CS = 52;
                id_CA = 102;
            }

            var pens_use_c = _context.Pensionars
               .Where(puc => puc.nrcarnet == nr_carnet_use_c).SingleOrDefault();

            string CekNrContD = "4614." + pens_use_c.nrcarnet.ToString();
            if (tip_document_rj == 2)
            {
                var getID_contA_461_ch = _context.conturi_analitice
                  .Where(girc461c => (girc461c.nr_cont_analitic.Trim() == CekNrContD)).SingleOrDefault();

                if (getID_contA_461_ch == null)
                {
                    _context.conturi_analitice.Add(new conturi_analitice
                    {
                        id_cont_sintetic = 26,
                        id_cont_analitic = Int32.Parse("4614" + pens_use_c.nrcarnet),
                        nr_cont_analitic = "4614." + nr_carnet_use_c.ToString(),
                        explicatie_nr_cont_analitic = pens_use_c.nume,
                    });

                    _context.SaveChanges();

                    _context.Pensionars.Add(new Pensionar
                    {
                        nrcarnet = Int32.Parse("4614" + pens_use_c.nrcarnet.ToString()),
                        nume = pens_use_c.nume,
                        prenume = pens_use_c.prenume,
                        cnp = pens_use_c.cnp,
                        pensie = pens_use_c.pensie,
                        nrcupon = pens_use_c.nrcupon,
                        taxainscriere = pens_use_c.taxainscriere,
                        datainscriere = pens_use_c.datainscriere,

                        judet = pens_use_c.judet,
                        localitate = pens_use_c.localitate,
                        strada = pens_use_c.strada,
                        nr = pens_use_c.nr,
                        bloc = pens_use_c.bloc,
                        ap = pens_use_c.ap,
                        telefon = pens_use_c.telefon,
                        email = pens_use_c.email,

                        sold_461 = pens_use_c.soldimp,
                        dataimp = pens_use_c.dataimp,
                        acimp = pens_use_c.acimp,
                        lunaimp = pens_use_c.lunaimp,
                        EXCL = "-",



                        id_stare = 3

                    });

                    pens_use_c.id_stare = 30;

                    _context.SaveChanges();

                }

                id_CS = 26;
                id_CA = Int32.Parse("4614" + pens_use_c.nrcarnet);
            }



            string CekNrContDP = "4611." + pens_use_c.nrcarnet.ToString();
            if (tip_document_rj == 4)
            {
                var getID_contA_461_ch = _context.conturi_analitice
                  .Where(girc461c => (girc461c.nr_cont_analitic.Trim() == CekNrContDP)).SingleOrDefault();

                if (getID_contA_461_ch == null)
                {
                    _context.conturi_analitice.Add(new conturi_analitice
                    {
                        id_cont_sintetic = 27,
                        id_cont_analitic = Int32.Parse("4611" + pens_use_c.nrcarnet),
                        nr_cont_analitic = "4611." + nr_carnet_use_c.ToString(),
                        explicatie_nr_cont_analitic = pens_use_c.nume,
                    });

                    _context.SaveChanges();

                    _context.Pensionars.Add(new Pensionar
                    {
                        nrcarnet = Int32.Parse("4611" + pens_use_c.nrcarnet.ToString()),
                        nume = pens_use_c.nume,
                        prenume = pens_use_c.prenume,
                        cnp = pens_use_c.cnp,
                        pensie = pens_use_c.pensie,
                        nrcupon = pens_use_c.nrcupon,
                        taxainscriere = pens_use_c.taxainscriere,
                        datainscriere = pens_use_c.datainscriere,

                        judet = pens_use_c.judet,
                        localitate = pens_use_c.localitate,
                        strada = pens_use_c.strada,
                        nr = pens_use_c.nr,
                        bloc = pens_use_c.bloc,
                        ap = pens_use_c.ap,
                        telefon = pens_use_c.telefon,
                        email = pens_use_c.email,

                        sold_461 = pens_use_c.soldimp,
                        dataimp = pens_use_c.dataimp,
                        acimp = pens_use_c.acimp,
                        lunaimp = pens_use_c.lunaimp,

                        solddobanda = dobanda_ch_use_c,
                        sold_DP = dp_ch_use,


                        
                        EXCL = "-",





                        id_stare = 4

                    });


                    _context.SaveChanges();

                }

                id_CS = 27;
                id_CA = Int32.Parse("4611" + pens_use_c.nrcarnet);
            }


            string CekNrContC = "4621." + pens_use_c.nrcarnet.ToString();
            if ((tip_document_rj == 51) || (tip_document_rj == 52) || (tip_document_rj == 5))
            {
                var getID_contA_462_ch = _context.conturi_analitice
                  .Where(girc462c => (girc462c.nr_cont_analitic.Trim() == CekNrContC)).SingleOrDefault();

                if (getID_contA_462_ch == null)
                {
                    _context.conturi_analitice.Add(new conturi_analitice
                    {
                        id_cont_sintetic = 18,
                        id_cont_analitic = Int32.Parse("4621" + pens_use_c.nrcarnet),
                        nr_cont_analitic = "4621." + nr_carnet_use_c.ToString(),
                        explicatie_nr_cont_analitic = pens_use_c.nume,
                    });

                    _context.SaveChanges();

                    id_CA = Int32.Parse("4621" + pens_use_c.nrcarnet);
                }

                id_CS = 18;

                if (getID_contA_462_ch != null)
                    id_CA = getID_contA_462_ch.id_cont_analitic;
            }


            var desf_use_c = _context.desfasurator_rate
                .Where(d_pt_use_c => ((d_pt_use_c.nrcarnet == nr_carnet_use_c) && (d_pt_use_c.data_rata < data_ch_use_c))).OrderBy(d_pt_use_c => d_pt_use_c.nr_rata).ToList();

            use_credit = pens_use_c.credit_imprumut;
            use_credit_initial = pens_use_c.credit_imprumut;

            if (rata_ch_use_c + use_credit == pens_use_c.soldimp)
            {
                iesire_462 = use_credit;
                iesire_2678 = use_credit;

                incasare_2678 = rata_ch_use_c - pens_use_c.debit_imprumut;
                incasare_461 = pens_use_c.debit_imprumut;

                check_lichidare = 1;

                update_desfasurator_end_sold(pens_use_c.nrcarnet, data_ch_use_c, idch_use_c, nrch_use_c, dobanda_ch_use_c);
            }


            if (check_lichidare == 0)
            {
                test_month_now = 0;
                foreach (desfasurator_rate d_pt_use_c in desf_use_c)
                {
                    if ((d_pt_use_c.data_rata.Month == data_ch_use_c.Month) && (d_pt_use_c.data_rata.Year == data_ch_use_c.Year)) test_month_now = 1;

                    if (test_month_now == 0)
                    {
                        test_new_record = 0;
                        dif_rata_pt_use_c = d_pt_use_c.rata_de_plata - d_pt_use_c.rata_platita;
                        //PA

                        if (desf_use_c.Count() > 0)
                            if (d_pt_use_c.rata_platita < d_pt_use_c.rata_de_plata)
                            {
                                if (use_credit + rata_ch_use_c >= dif_rata_pt_use_c)
                                {
                                    d_pt_use_c.rata_platita = d_pt_use_c.rata_de_plata;
                                    _context.SaveChanges();
                                }

                                if (use_credit + rata_ch_use_c < dif_rata_pt_use_c)
                                {
                                    d_pt_use_c.rata_platita = d_pt_use_c.rata_platita + use_credit + rata_ch_use_c;
                                    _context.SaveChanges();
                                }

                                if ((rata_ch_use_c >= dif_rata_pt_use_c) && (test_new_record == 0))
                                {
                                    incasare_461 = incasare_461 + dif_rata_pt_use_c;

                                    rata_ch_use_c = rata_ch_use_c - dif_rata_pt_use_c;
                                    test_new_record = 1;
                                }

                                if ((rata_ch_use_c < dif_rata_pt_use_c) && (test_new_record == 0))
                                {
                                    incasare_461 = incasare_461 + rata_ch_use_c;

                                    rata_ch_use_c = 0;
                                    test_new_record = 1;
                                }
                            }
                    }
                }

                //end foreach

                //DE AICI APR 2023

                if (rata_ch_use_c >= (rata_ch_de_plata_pt_use_c - rata_platita_ch_use_c))
                {
                    incasare_2678 = incasare_2678 + rata_ch_de_plata_pt_use_c - rata_platita_ch_use_c;
                    incasare_462 = incasare_462 + (rata_ch_use_c - (rata_ch_de_plata_pt_use_c - rata_platita_ch_use_c));

                    rata_ch_use_c = rata_ch_de_plata_pt_use_c;

                }

                test_new_record = 0;

                dif_rata_pt_use_c = rata_ch_de_plata_pt_use_c - rata_platita_ch_use_c - rata_ch_use_c;

                if (dif_rata_pt_use_c > 0)
                {
                    if (use_credit >= 0)
                    {
                        //OK
                        if ((use_credit >= dif_rata_pt_use_c) && (test_new_record == 0))
                        {
                            iesire_462 = iesire_462 + dif_rata_pt_use_c;
                            iesire_2678 = iesire_2678 + dif_rata_pt_use_c;

                            use_credit = use_credit - dif_rata_pt_use_c;
                            incasare_2678 = incasare_2678 + rata_ch_use_c;

                            rata_ch_use_c = rata_ch_de_plata_pt_use_c;

                            test_new_record = 1;
                        }

                        if ((use_credit < dif_rata_pt_use_c) && (test_new_record == 0))
                        {
                            //da iun 2023
                            var exista_ch_month_use_credit = _context.Chitantas
                                                            .Where(vcmuc => (vcmuc.nrcarnet == nr_carnet_use_c) && (vcmuc.data.Month == data_ch_use_c.Month) && (vcmuc.data.Year == data_ch_use_c.Year) && (vcmuc.rata > 0));

                            iesire_462 = iesire_462 + use_credit;
                            iesire_2678 = iesire_2678 + use_credit;

                            if (exista_ch_month_use_credit.Count() == 1)
                                incasare_2678 = incasare_2678 + rata_ch_use_c;
                            if (exista_ch_month_use_credit.Count() > 1)
                                incasare_461 = incasare_461 + rata_ch_use_c;

                            if (exista_ch_month_use_credit.Count() == 1)
                                intrare_461 = intrare_461 + (rata_ch_de_plata_pt_use_c - rata_ch_use_c - use_credit - rata_platita_ch_use_c);

                            rata_ch_use_c = rata_ch_use_c + use_credit + rata_platita_ch_use_c;
                            use_credit = 0;
                            test_new_record = 1;
                        }

                    }

                }

                //PA APRILIRE 2023
            }



            if (incasare_461 != 0)
            {
                if (tip_document_rj == 0)
                {

                    _context.registru_casa.Add(new registru_casa
                    {
                        data = data_ch_use_c,
                        nr_document = nrch_use_c.ToString(),
                        id_chimpr = idch_use_c,
                        id_cont_sintetic = 107,
                        id_cont_analitic = Int32.Parse("4612" + nr_carnet_use_c.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        sold_initial = pens_use_c.debit_imprumut,
                        suma = incasare_461,
                        intrare = 0,
                        incasare = incasare_461,
                        iesire = 0,
                        plata = 0,
                        sold_final = pens_use_c.debit_imprumut - incasare_461,
                        tip = "incasare",
                        sortare = "H"
                    });
                }

                // if (get_iduser_rj_2678 < 9)

                pens_use_c.debit_imprumut = pens_use_c.debit_imprumut - incasare_461;
                pens_use_c.soldimp = pens_use_c.soldimp - incasare_461;
                pens_use_c.restimp = pens_use_c.restimp + incasare_461;

                _context.SaveChanges();

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_ch_use_c,
                    id_document = idch_use_c,
                    nr_document = nrch_use_c.ToString(),
                    id_CS_debitor = id_CS,
                    id_CA_debitor = id_CA,
                    id_CS_creditor = 107,
                    id_CA_creditor = Int32.Parse("4612" + pens_use_c.nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_use_c.debit_imprumut + incasare_461,
                    debit = incasare_461,
                    credit = incasare_461,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_use_c.debit_imprumut,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();
            }


            if (iesire_462 != 0)
            {
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_ch_use_c,
                    id_document = idch_use_c,
                    nr_document = nrch_use_c.ToString(),
                    id_CS_debitor = 106,
                    id_CA_debitor = Int32.Parse("4622" + pens_use_c.nrcarnet.ToString()),
                    id_CS_creditor = 103,
                    id_CA_creditor = Int32.Parse("2678" + pens_use_c.nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = pens_use_c.credit_imprumut,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_use_c.soldimp,
                    debit = iesire_462,
                    credit = iesire_462,
                    SF_CS_debit = 0,
                    SF_CA_debit = pens_use_c.credit_imprumut - iesire_462,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_use_c.soldimp - iesire_462,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                pens_use_c.credit_imprumut = pens_use_c.credit_imprumut - iesire_462;
                pens_use_c.soldimp = pens_use_c.soldimp - iesire_462;
                pens_use_c.restimp = pens_use_c.restimp + iesire_462;

                _context.SaveChanges();

            }


            if (incasare_462 != 0)
            {
                if (tip_document_rj == 0)
                {
                    _context.registru_casa.Add(new registru_casa
                    {
                        data = data_ch_use_c,
                        nr_document = nrch_use_c.ToString(),
                        id_chimpr = idch_use_c,
                        id_cont_sintetic = 106,
                        id_cont_analitic = Int32.Parse("4622" + nr_carnet_use_c.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        sold_initial = pens_use_c.credit_imprumut,
                        suma = incasare_462,
                        incasare = incasare_462,
                        iesire = 0,
                        plata = 0,
                        sold_final = pens_use_c.credit_imprumut + incasare_462,
                        tip = "incasare",
                        sortare = "H"
                    });
                }

                pens_use_c.credit_imprumut = pens_use_c.credit_imprumut + incasare_462;

                _context.SaveChanges();

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_ch_use_c,
                    id_document = idch_use_c,
                    nr_document = nrch_use_c.ToString(),
                    id_CS_debitor = id_CS,
                    id_CA_debitor = id_CA,
                    id_CS_creditor = 106,
                    id_CA_creditor = Int32.Parse("4622" + pens_use_c.nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_use_c.credit_imprumut - incasare_462,
                    debit = incasare_462,
                    credit = incasare_462,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_use_c.credit_imprumut,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();

            }


            if (incasare_2678 != 0)
            {

                if (tip_document_rj == 0)
                {
                    _context.registru_casa.Add(new registru_casa
                    {
                        data = data_ch_use_c,
                        nr_document = nrch_use_c.ToString(),
                        id_chimpr = idch_use_c,
                        id_cont_sintetic = 103,
                        id_cont_analitic = Int32.Parse("2678" + nr_carnet_use_c.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        sold_initial = pens_use_c.soldimp,
                        suma = incasare_2678,
                        incasare = incasare_2678,
                        iesire = 0,
                        plata = 0,
                        sold_final = pens_use_c.soldimp - incasare_2678,
                        tip = "incasare",
                        sortare = "H"
                    });
                }

                pens_use_c.soldimp = pens_use_c.soldimp - incasare_2678;
                pens_use_c.restimp = pens_use_c.restimp + incasare_2678;

                _context.SaveChanges();

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_ch_use_c,
                    id_document = idch_use_c,
                    nr_document = nrch_use_c.ToString(),
                    id_CS_debitor = id_CS,
                    id_CA_debitor = id_CA,
                    id_CS_creditor = 103,
                    id_CA_creditor = Int32.Parse("2678" + pens_use_c.nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_use_c.soldimp + incasare_2678,
                    debit = incasare_2678,
                    credit = incasare_2678,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_use_c.soldimp,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                _context.SaveChanges();

            }


            if (iesire_461 != 0)
            {

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_ch_use_c,
                    id_document = idch_use_c,
                    nr_document = nrch_use_c.ToString(),
                    id_CS_debitor = 103,
                    id_CA_debitor = Int32.Parse("2678" + pens_use_c.nrcarnet.ToString()),
                    id_CS_creditor = 107,
                    id_CA_creditor = Int32.Parse("4612" + pens_use_c.nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = pens_use_c.soldimp,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_use_c.debit_imprumut,
                    debit = iesire_461,
                    credit = iesire_461,
                    SF_CS_debit = 0,
                    SF_CA_debit = pens_use_c.soldimp - iesire_461,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_use_c.debit_imprumut,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                // if (get_iduser_rj_2678 < 9)

                pens_use_c.debit_imprumut = pens_use_c.debit_imprumut - iesire_461;
                pens_use_c.soldimp = pens_use_c.soldimp - iesire_461;
                pens_use_c.restimp = pens_use_c.restimp + iesire_461;

                _context.SaveChanges();

            }


            if (intrare_461 != 0)
            {

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_ch_use_c,
                    id_document = idch_use_c,
                    nr_document = nrch_use_c.ToString(),
                    id_CS_debitor = 107,
                    id_CA_debitor = Int32.Parse("4612" + pens_use_c.nrcarnet.ToString()),
                    id_CS_creditor = 103,
                    id_CA_creditor = Int32.Parse("2678" + pens_use_c.nrcarnet.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = pens_use_c.debit_imprumut,
                    SI_CS_credit = 0,
                    SI_CA_credit = pens_use_c.soldimp,
                    debit = intrare_461,
                    credit = intrare_461,
                    SF_CS_debit = 0,
                    SF_CA_debit = pens_use_c.debit_imprumut + intrare_461,
                    SF_CS_credit = 0,
                    SF_CA_credit = pens_use_c.soldimp,
                    tip_document = tip_document_rj,
                    sortare = "H"
                });

                pens_use_c.debit_imprumut = pens_use_c.debit_imprumut + intrare_461;

                _context.SaveChanges();

                _context.registru_casa.Add(new registru_casa
                {
                    data = data_ch_use_c,
                    nr_document = nrch_use_c.ToString(),
                    id_chimpr = idch_use_c,
                    id_cont_sintetic = 107,
                    id_cont_analitic = Int32.Parse("4612" + nr_carnet_use_c.ToString()),
                    explicatie_nr_cont_analitic = "-",
                    sold_initial = pens_use_c.debit_imprumut - intrare_461,
                    suma = intrare_461,
                    incasare = 0,
                    intrare = intrare_461,
                    plata = 0,
                    sold_final = pens_use_c.debit_imprumut,
                    tip = "intrare",
                    sortare = "H"
                });

                _context.SaveChanges();

            }


            //credit 
            idcredit = Int32.Parse("4622" + pens_use_c.nrcarnet.ToString());

            var checkIdCredit = _context.conturi_analitice
                .Where(cic => cic.id_cont_analitic == idcredit);

            if (checkIdCredit.Count() == 0)
            {
                _context.conturi_analitice.Add(new conturi_analitice
                {
                    id_cont_analitic = Int32.Parse("4622" + pens_use_c.nrcarnet.ToString()),
                    id_cont_sintetic = 106,
                    nr_cont_analitic = "4622." + pens_use_c.nrcarnet.ToString(),
                    explicatie_nr_cont_analitic = pens_use_c.nume
                });
                _context.SaveChanges();
            }

            //debit
            iddebit = Int32.Parse("4612" + pens_use_c.nrcarnet.ToString());

            var checkIdDebit = _context.conturi_analitice
                .Where(cic => cic.id_cont_analitic == iddebit);

            if (checkIdDebit.Count() == 0)
            {
                _context.conturi_analitice.Add(new conturi_analitice
                {
                    id_cont_analitic = Int32.Parse("4612" + pens_use_c.nrcarnet.ToString()),
                    id_cont_sintetic = 107,
                    nr_cont_analitic = "4612." + pens_use_c.nrcarnet.ToString(),
                    explicatie_nr_cont_analitic = pens_use_c.nume,
                });
                _context.SaveChanges();
            }



            return rata_ch_use_c;
        }

        public int update_desfasurator_end_sold(int nrcarnet_desf_end_sold, DateTime data_end_desf, int idch_end_desf, int nrch_end_desf, decimal dobanda_end_desf)
        {

            //   IUNIE 2024 TABLETE

            decimal dobanda_minus = 0;
            int test_data_end_desf;

            var desf_end_sold = _context.desfasurator_rate
                .Where(des => des.nrcarnet == nrcarnet_desf_end_sold).OrderBy(des => des.data_rata).ToList();

            var pens_end_sold = _context.Pensionars.Where(ped => ped.nrcarnet == nrcarnet_desf_end_sold).SingleOrDefault();

            dobanda_minus = 0;

            foreach (desfasurator_rate des in desf_end_sold)
            {

                test_data_end_desf = 0;

                _context.desfasurator_rate_arhiva.Add(new desfasurator_rate_arhiva
                {
                    id_imprumut = des.id_imprumut,
                    nrcarnet = des.nrcarnet,
                    nr_rata = des.nr_rata,
                    nr_rata_noua = des.nr_rata_noua,
                    data_rata = des.data_rata,
                    sold_imp = des.sold_imp,
                    rata_de_plata = des.rata_de_plata,
                    rata_platita = des.rata_platita,
                    rata_platita_initial = des.rata_platita_initial,
                    dobanda = des.dobanda,
                    total = des.total,
                    nr_desfasurator = des.nr_desfasurator
                });

                if ((des.data_rata.Month == data_end_desf.Month) && (des.data_rata.Year == data_end_desf.Year)) test_data_end_desf = 1;

                if (des.data_rata > data_end_desf)
                    if (test_data_end_desf == 0)
                        dobanda_minus = dobanda_minus + des.dobanda;

                _context.desfasurator_rate.Remove(des);

                _context.SaveChanges();
            }

            _context.registru_jurnal.Add(new registru_jurnal
            {
                data = data_end_desf,
                id_document = idch_end_desf,
                nr_document = nrch_end_desf.ToString(),
                id_CS_debitor = 109,
                id_CA_debitor = Int32.Parse("6791" + nrcarnet_desf_end_sold.ToString()),
                id_CS_creditor = 53,
                id_CA_creditor = 103,
                explicatie_nr_cont_analitic = "LA",
                SI_CS_debit = 0,
                SI_CA_debit = dobanda_minus + dobanda_end_desf,
                SI_CS_credit = 0,
                SI_CA_credit = 0,
                debit = -dobanda_minus,
                credit = -dobanda_minus,
                SF_CS_debit = 0,
                SF_CA_debit = 0,
                SF_CS_credit = 0,
                SF_CA_credit = 0,
                tip_document = 11,
                sortare = "H"

            });

            pens_end_sold.solddobanda = 0;
            _context.SaveChanges();

            return 0;
        }


        public async Task<IActionResult> ListareCH(int idListCH, int id_user_list_ch)
        {
            int nrch = 0;


            var getCHList = _context.Chitantas
                .Where(gcl => gcl.idchitanta == idListCH).SingleOrDefault();

            nrch = getCHList.nrch;


            MemoryStream ms = new MemoryStream();
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A5.Rotate(), false);
            writer.SetCloseStream(false);


            document.SetMargins(40f, 20f, 10f, 40f);


            //de aici
            //iText.Kernel.Geom.Rectangle RdelegatiCH = new iText.Kernel.Geom.Rectangle(211, 450);           
            //PageSize pgSizeD = new PageSize(RdelegatiCH);           
            if (id_user_list_ch > 9)
            {
                //document = new Document(pdfDoc, pgSizeD, false);
                document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A7, false);
                document.SetMargins(5f, 5f, 5f, 5f);
            }

            if (id_user_list_ch < 10)
            {
                document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A5.Rotate(), false);
                document.SetMargins(50f, 20f, 10f, 50f);
            }


            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            normal.SetFont(font).SetFontSize(14);



            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT).SetFixedLeading(10)
                .SetBold().SetFontSize(13).AddStyle(normal);
            document.Add(header1);


            //document.SetFontFamily(bfTimes);

            Paragraph header2 = new Paragraph("cod fiscal 7230589")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(13)
                 .SetBold().AddStyle(normal).SetFixedLeading(10);
            document.Add(header2);

            document.Add(new Paragraph(" "));

            //pa
            //adaug tabel
            //  document.Add(await (GetPdfRCZilnic()));


            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            normalTbl.SetFont(fontTbl).SetFontSize(12);


            iText.Layout.Element.Table table = new iText.Layout.Element.Table(2, true);


            //table.SetWidth(600);

            //("dd.MM.yyyy hh:mm:ss tt")  + " </td></tr>" +

            Cell nrch_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("nr chitanta").AddStyle(normalTbl));
            nrch_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //nrch_titlu.SetHeight(15);
            table.AddCell(nrch_titlu);
            Cell nr_ch = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.nrch.ToString()).AddStyle(normalTbl));
            nr_ch.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
           // nr_ch.SetHeight(15);
            table.AddCell(nr_ch);


            Cell nr_fisa_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("Nr fisa").AddStyle(normalTbl).SetBold());
            nr_fisa_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //nr_fisa_titlu.SetHeight(15);
            table.AddCell(nr_fisa_titlu);
            Cell nr_fisa = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.nrcarnet.ToString()).AddStyle(normalTbl));
            nr_fisa.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //nr_fisa.SetHeight(15);
            table.AddCell(nr_fisa);



            Cell nume_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("Dl (D-na)").AddStyle(normalTbl)).SetBold();
            nume_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //nume_titlu.SetHeight(10);
            table.AddCell(nume_titlu);
            Cell nume = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.nume).SetBold().AddStyle(normalTbl));
            nume.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //nume.SetHeight(10);
            table.AddCell(nume);



            Cell data_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("data").AddStyle(normalTbl));
            data_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //data_titlu.SetHeight(20);
            table.AddCell(data_titlu);
            Cell data = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.data.ToString("dd.MM.yyyy hh:mm:ss tt")).AddStyle(normalTbl));
            data.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //data.SetHeight(20);
            table.AddCell(data);


            Cell cotizatie_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("cotizatie").AddStyle(normalTbl));
            cotizatie_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //cotizatie_titlu.SetHeight(15);
            table.AddCell(cotizatie_titlu);
            Cell cotizatie = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.cotizatie.ToString("N2")).AddStyle(normalTbl));
            cotizatie.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //cotizatie.SetHeight(15);
            table.AddCell(cotizatie);



            Cell contributie_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("contributie").AddStyle(normalTbl));
            contributie_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //contributie_titlu.SetHeight(15);
            table.AddCell(contributie_titlu);
            Cell contributie = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.ajdeces.ToString("N2")).AddStyle(normalTbl));
            contributie.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //contributie.SetHeight(15);
            table.AddCell(contributie);




            Cell perioada_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("perioada").AddStyle(normalTbl));
            perioada_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //perioada_titlu.SetHeight(15);
            table.AddCell(perioada_titlu);
            Cell perioada = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.lunaajdeces.ToString("dd.MM.yyyy")).AddStyle(normalTbl));
            perioada.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //perioada.SetHeight(15);
            table.AddCell(perioada);





            Cell rata_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph("rata impr.").AddStyle(normalTbl));
            rata_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //rata_titlu.SetHeight(15);
            table.AddCell(rata_titlu);
            Cell rata = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.rata.ToString("N2")).AddStyle(normalTbl));
            rata.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //rata.SetHeight(15);
            table.AddCell(rata);






            Cell dp_titlu = new Cell(1, 1)
                .SetTextAlignment(TextAlignment.LEFT)
                 .Add(new Paragraph("dobanda penalizatoare").AddStyle(normalTbl));
            dp_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //dp_titlu.SetHeight(15);
            table.AddCell(dp_titlu);
            Cell dobanda_penalizatoare = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.dobanda_penalizatoare.ToString("N2")).AddStyle(normalTbl));
            dobanda_penalizatoare.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //dobanda_penalizatoare.SetHeight(15);
            table.AddCell(dobanda_penalizatoare);


            Cell dobanda_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.LEFT)
                .Add(new Paragraph("dobanda").AddStyle(normalTbl));
            dobanda_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //dobanda_titlu.SetHeight(15);
            table.AddCell(dobanda_titlu);
            Cell dobanda = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.dobanda.ToString("N2")).AddStyle(normalTbl));
            dobanda.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //dobanda.SetHeight(15);
            table.AddCell(dobanda);


                      




            Cell taxainscr_titlu = new Cell(1, 1)
            .SetTextAlignment(TextAlignment.LEFT)
             .Add(new Paragraph("taxa inscriere").AddStyle(normalTbl));
            taxainscr_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //taxainscr_titlu.SetHeight(15);
            table.AddCell(taxainscr_titlu);
            Cell taxa_inscriere = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.taxainscr.ToString("N2")).AddStyle(normalTbl));
            taxa_inscriere.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            // taxa_inscriere.SetHeight(15);
            table.AddCell(taxa_inscriere);

            Cell t_titlu = new Cell(1, 1)
            .SetTextAlignment(TextAlignment.LEFT)
             .Add(new Paragraph("Total").AddStyle(normalTbl).SetBold());
            t_titlu.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            //taxainscr_titlu.SetHeight(15);
            table.AddCell(t_titlu);
            Cell tt = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.LEFT)
                     .Add(new Paragraph(getCHList.total.ToString("N2")).AddStyle(normalTbl).SetBold());
            tt.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);
            // taxa_inscriere.SetHeight(15);
            table.AddCell(tt);


            document.Add(table);
            table.Complete();

            document.Add(new Paragraph(" "));

            Paragraph footer1 = new Paragraph("Casier")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetBold().SetFontSize(12).SetBold().AddStyle(normal);
            document.Add(footer1);


            document.Close();
            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;
            
            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");
            // using (FileStream fs = File.Create("C:\\test.pdf")) { fs.Write(byteInfo, 0, (int)byteInfo.Length); } 

            //File(byteInfo, "application/pdf", "E:\\JOB\\test.pdf");
            return fileStreamResult;

        }


        public ActionResult incasari_zilnice(string zi = "01/01/2015", string nrch1 = "0", string nrch2 = "0", int delegat = 1, int? page = 1)
        {
            //IULIE 2024 TABLETS    net 8                 
            int nr1, nr2;
            DateTime ziua;
            decimal sumrata = 0, sumdobanda = 0, sumcotizatie = 0, sumtaxa = 0, sumtotal = 0, sumajdeces = 0;


            if (DateTime.TryParse(zi, out ziua) == false)
            {
                ViewBag.msgerr = "Introduceti o valoare calendaristica pentru ziua";
                return View("Error");
            }


            if (int.TryParse(nrch1, out nr1) == false)
            {
                ViewBag.msgerr = "Introduceti o valoare numerica pentru 'de la nr'";
                return View("Error");
            }


            if (int.TryParse(nrch2, out nr2) == false)
            {
                ViewBag.msgerr = "Introduceti o valoare numerica pentru 'la nr'";
                return View("Error");
            }

            @ViewBag.cotizatie = "0.00";
            @ViewBag.rata = "0.00";
            @ViewBag.dobanda = "0.00";
            @ViewBag.ajdeces = "0.00";
            @ViewBag.taxa = "0.00";
            @ViewBag.total = "0.00";
          

            ViewData["nrch1"] = nrch1;
           ViewData["nrch2"]= nrch2;
            ViewData["zi"] = zi;


            var incasari = _context.Chitantas
                        .Where(i => (i.data.Day == ziua.Day) && (i.data.Month == ziua.Month) && (i.data.Year == ziua.Year) && (i.nrch >= nr1) && (i.nrch <= nr2) && (i.cotizatie >= 0) && (i.ajdeces >= 0));

                      int get_iduser_IncasZi = 0;

            string userId = User.Identity.Name.Trim();
            var getIDUserIncasZi = _context.utilizatoris
                           .Where(p => p.nume_user == userId).FirstOrDefault();
            
            if (delegat == 1) get_iduser_IncasZi = 2;
            if (delegat == 3) get_iduser_IncasZi = 17;
            if (delegat == 2) get_iduser_IncasZi = 16;
            if (delegat == 5) get_iduser_IncasZi = 19;
            if (delegat == 4) get_iduser_IncasZi = 18;
            if (delegat == 6) get_iduser_IncasZi = 20;
            if (delegat == 7) get_iduser_IncasZi = 22;
            if (getIDUserIncasZi.IDUser > 10) get_iduser_IncasZi = getIDUserIncasZi.IDUser;
            if (get_iduser_IncasZi > 9) incasari = incasari.Where(id => (id.id_utilizator == get_iduser_IncasZi)).OrderBy(id => id.nrch);
            if ((get_iduser_IncasZi <= 9) && (delegat == 1)) incasari = incasari.Where(id => (id.id_utilizator < 10)).OrderBy(id => id.nrch);
           
            ViewData["delegat"] = get_iduser_IncasZi;
            ViewData["delegat"] = delegat;

            if (incasari.Count() > 0)
            {

                decimal totalcotizatie = incasari.Sum(i => i.cotizatie);
                sumcotizatie = totalcotizatie;

                decimal totalrata = incasari.Sum(i => i.rata);
                sumrata = totalrata;

                decimal totaldobanda = incasari.Sum(i => i.dobanda);
                sumdobanda = totaldobanda;

                decimal totalajdeces = incasari.Sum(i => i.ajdeces);
                sumajdeces = totalajdeces;

                decimal totaltaxa = incasari.Sum(i => i.taxainscr);
                sumtaxa = totaltaxa;

                decimal totaltotal = incasari.Sum(i => i.total);
                sumtotal = totaltotal;

                decimal totaldobpen = incasari.Sum(i => i.dobanda_penalizatoare);                                          
              

                @ViewBag.cotizatie = totalcotizatie.ToString("N2");
                @ViewBag.rata = totalrata.ToString("N2");
                @ViewBag.dobanda = totaldobanda.ToString("N2");
                @ViewBag.ajdeces = totalajdeces.ToString("N2"); ;
                @ViewBag.taxa = totaltaxa.ToString("N2");
                @ViewBag.dobanda_penalizatoare = totaldobpen.ToString("N2");
                @ViewBag.total = totaltotal.ToString("N2");

            }


         



            var pageNumber = page ?? 1;
            ViewBag.incasari_zilnice = incasari.ToList().ToPagedList(pageNumber, 12);
        
            return View(ViewBag.incasari_zilnice);

        }
             
          

        public int insert_audit_chitanta(int User, int idch, int nrchitantach, DateTime datach, int nrcarnetch, decimal ratach, decimal dobandach, decimal cotizatiech, decimal contributiech, DateTime lunaajdecesch, string editareIn)
        {
            //IULIE 2023 TABLETS MVC
            _context.Audit_Chitanta.Add(new Audit_Chitanta
            {

                tip_operatie = "Edit",
                data = DateTime.Now,
                utilizator = User,
                idchitanta = idch,
                nrchitanta = nrchitantach,
                data_chitanta = datach,
                nrcarnet = nrcarnetch,
                rata = ratach,
                dobanda = dobandach,
                cotizatie = cotizatiech,
                contributie = contributiech,
                luna_ajutor_deces = lunaajdecesch,
                editare = editareIn
            });


            _context.SaveChanges();
            return 0;
        }




    }

}
