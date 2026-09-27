'use strict';
const $ = id => document.getElementById(id);
let allBooks = [], currentBooks = [], loadVersion = 0, detailVersion = 0;
const normalize = s => String(s ?? '').normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
async function getJson(url) {
  const response = await fetch(url, {headers:{Accept:'application/json'}});
  if (!response.ok) throw new Error(response.status === 404 ? 'No existe un libro con ese identificador.' : 'No se pudo consultar el catálogo. Comprueba que SQL Server esté disponible y la conexión configurada.');
  return response.json();
}
function render() {
  const term = normalize($('search').value.trim()), year = $('year').value;
  const books = currentBooks.filter(b => (!year || String(b.anioPublicacion) === year) &&
    (!term || normalize([b.titulo,b.isbn,b.autor.nombre].join(' ')).includes(term)));
  $('books').replaceChildren();
  for (const book of books) {
    const card = $('book-template').content.cloneNode(true);
    for (const [selector,text] of Object.entries({'.cover-category':book.categoria.nombre,'.cover-title':book.titulo,
      '.cover-author':book.autor.nombre,'.tag':book.categoria.nombre,'h3':book.titulo,
      '.author':book.autor.nombre,'.metadata':book.anioPublicacion+' · ISBN '+book.isbn})) card.querySelector(selector).textContent=text;
    const button=card.querySelector('button');
    button.setAttribute('aria-label','Ver detalle de '+book.titulo);
    button.addEventListener('click',()=>openDetail(book.id));
    $('books').append(card);
  }
  $('status').textContent=books.length+' libro'+(books.length===1?'':'s')+' encontrado'+(books.length===1?'':'s');
  $('empty').hidden=books.length>0;
}
function populateFilters() {
  const categories=new Map(allBooks.map(b=>[b.categoria.id,b.categoria.nombre]));
  $('category').replaceChildren(new Option('Todas las categorías',''));
  [...categories].sort((a,b)=>a[1].localeCompare(b[1],'es')).forEach(([id,name])=>$('category').add(new Option(name,id)));
  $('year').replaceChildren(new Option('Todos los años',''));
  [...new Set(allBooks.map(b=>b.anioPublicacion))].sort((a,b)=>b-a).forEach(y=>$('year').add(new Option(y,y)));
}
async function loadCatalog() {
  const version=++loadVersion;
  $('error').hidden=true; $('empty').hidden=true;
  $('books').replaceChildren(); $('books').setAttribute('aria-busy','true'); $('status').textContent='Cargando libros…';
  $('reload').disabled=true; $('category').disabled=true;
  try {
    const books=await getJson('/api/libros');
    if(version!==loadVersion)return;
    allBooks=books; currentBooks=books; populateFilters(); render();
  } catch(error) {
    if(version!==loadVersion)return;
    allBooks=[];currentBooks=[];populateFilters();
    $('error').textContent=error.message; $('error').hidden=false; $('status').textContent='Catálogo no disponible';
  } finally {if(version===loadVersion){$('books').setAttribute('aria-busy','false');$('reload').disabled=false;$('category').disabled=false;}}
}
async function filterCategory() {
  const version=++loadVersion, id=$('category').value;
  $('error').hidden=true; $('empty').hidden=true; $('books').replaceChildren();
  $('status').textContent='Consultando categoría…'; $('books').setAttribute('aria-busy','true');
  currentBooks=[];
  try {
    const books=id ? await getJson('/api/categorias/'+encodeURIComponent(id)+'/libros') : allBooks;
    if(version!==loadVersion)return;
    currentBooks=books; render();
  } catch(error) {if(version===loadVersion){$('error').textContent=error.message;$('error').hidden=false;$('status').textContent='No se pudo consultar la categoría';}}
  finally {if(version===loadVersion)$('books').setAttribute('aria-busy','false');}
}
async function openDetail(id) {
  const version=++detailVersion;
  $('detail-title').textContent='Consultando…';$('detail-body').replaceChildren();
  if(!$('detail').open)$('detail').showModal();
  try {
    const book=await getJson('/api/libros/'+encodeURIComponent(id));
    if(version!==detailVersion)return;
    $('detail-title').textContent=book.titulo;
    const dl=document.createElement('dl');
    for(const [label,value] of [['Autor',book.autor.nombre],['Categoría',book.categoria.nombre],['ISBN',book.isbn],['Publicación',book.anioPublicacion],['ID',book.id]]) {
      const dt=document.createElement('dt'),dd=document.createElement('dd');
      dt.textContent=label;dd.textContent=value;dl.append(dt,dd);
    }
    const description=document.createElement('p');description.className='description';description.textContent=book.descripcion;
    $('detail-body').append(dl,description);
  } catch(error) {if(version===detailVersion){$('detail-title').textContent='Consulta sin resultado';$('detail-body').textContent=error.message;}}
}
$('search').addEventListener('input',render);$('year').addEventListener('change',render);
$('category').addEventListener('change',filterCategory);
$('filters').addEventListener('submit',e=>e.preventDefault());
$('filters').addEventListener('reset',()=>{setTimeout(()=>{++loadVersion;currentBooks=allBooks;$('error').hidden=true;$('books').setAttribute('aria-busy','false');render();},0);});
$('reload').addEventListener('click',loadCatalog);
$('close-detail').addEventListener('click',()=>$('detail').close());
$('detail').addEventListener('close',()=>{++detailVersion;});
$('lookup').addEventListener('submit',event=>{
  event.preventDefault();const id=$('book-id').value.trim();$('lookup-error').textContent='';
  if(!/^[0-9a-f]{8}-(?:[0-9a-f]{4}-){3}[0-9a-f]{12}$/i.test(id)){$('lookup-error').textContent='Escribe un ID válido con el formato indicado.';return;}
  openDetail(id);
});
loadCatalog();

